#define DEBUG
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryLocal
{
	/// <summary>
	/// Memory.dll class. Full documentation at https://github.com/erfg12/memory.dll/wiki
	/// </summary>
	public class Mem
	{
		private enum SnapshotFlags : uint
		{
			HeapList = 1u,
			Process = 2u,
			Thread = 4u,
			Module = 8u,
			Module32 = 0x10u,
			Inherit = 0x80000000u,
			All = 0x1Fu,
			NoHeaps = 0x40000000u
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct PROCESSENTRY32
		{
			private const int MAX_PATH = 260;

			internal uint dwSize;

			internal uint cntUsage;

			internal uint th32ProcessID;

			internal IntPtr th32DefaultHeapID;

			internal uint th32ModuleID;

			internal uint cntThreads;

			internal uint th32ParentProcessID;

			internal int pcPriClassBase;

			internal uint dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			internal string szExeFile;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct MODULEENTRY32
		{
			internal uint dwSize;

			internal uint th32ModuleID;

			internal uint th32ProcessID;

			internal uint GlblcntUsage;

			internal uint ProccntUsage;

			internal IntPtr modBaseAddr;

			internal uint modBaseSize;

			internal IntPtr hModule;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			internal string szModule;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			internal string szExePath;
		}

		internal enum MINIDUMP_TYPE
		{
			MiniDumpNormal = 0,
			MiniDumpWithDataSegs = 1,
			MiniDumpWithFullMemory = 2,
			MiniDumpWithHandleData = 4,
			MiniDumpFilterMemory = 8,
			MiniDumpScanMemory = 0x10,
			MiniDumpWithUnloadedModules = 0x20,
			MiniDumpWithIndirectlyReferencedMemory = 0x40,
			MiniDumpFilterModulePaths = 0x80,
			MiniDumpWithProcessThreadData = 0x100,
			MiniDumpWithPrivateReadWriteMemory = 0x200,
			MiniDumpWithoutOptionalData = 0x400,
			MiniDumpWithFullMemoryInfo = 0x800,
			MiniDumpWithThreadInfo = 0x1000,
			MiniDumpWithCodeSegs = 0x2000
		}

		[Flags]
		public enum MemoryProtection : uint
		{
			Execute = 0x10u,
			ExecuteRead = 0x20u,
			ExecuteReadWrite = 0x40u,
			ExecuteWriteCopy = 0x80u,
			NoAccess = 0x1u,
			ReadOnly = 0x2u,
			ReadWrite = 0x4u,
			WriteCopy = 0x8u,
			GuardModifierflag = 0x100u,
			NoCacheModifierflag = 0x200u,
			WriteCombineModifierflag = 0x400u
		}

		[Flags]
		public enum ThreadAccess
		{
			TERMINATE = 0x1,
			SUSPEND_RESUME = 0x2,
			GET_CONTEXT = 0x8,
			SET_CONTEXT = 0x10,
			SET_INFORMATION = 0x20,
			QUERY_INFORMATION = 0x40,
			SET_THREAD_TOKEN = 0x80,
			IMPERSONATE = 0x100,
			DIRECT_IMPERSONATION = 0x200
		}

		public struct SYSTEM_INFO
		{
			public ushort processorArchitecture;

			private ushort reserved;

			public uint pageSize;

			public UIntPtr minimumApplicationAddress;

			public UIntPtr maximumApplicationAddress;

			public IntPtr activeProcessorMask;

			public uint numberOfProcessors;

			public uint processorType;

			public uint allocationGranularity;

			public ushort processorLevel;

			public ushort processorRevision;
		}

		public struct MEMORY_BASIC_INFORMATION32
		{
			public UIntPtr BaseAddress;

			public UIntPtr AllocationBase;

			public uint AllocationProtect;

			public uint RegionSize;

			public uint State;

			public uint Protect;

			public uint Type;
		}

		public struct MEMORY_BASIC_INFORMATION64
		{
			public UIntPtr BaseAddress;

			public UIntPtr AllocationBase;

			public uint AllocationProtect;

			public uint __alignment1;

			public ulong RegionSize;

			public uint State;

			public uint Protect;

			public uint Type;

			public uint __alignment2;
		}

		public struct MEMORY_BASIC_INFORMATION
		{
			public UIntPtr BaseAddress;

			public UIntPtr AllocationBase;

			public uint AllocationProtect;

			public long RegionSize;

			public uint State;

			public uint Protect;

			public uint Type;
		}

		private const int PROCESS_CREATE_THREAD = 2;

		private const int PROCESS_QUERY_INFORMATION = 1024;

		private const int PROCESS_VM_OPERATION = 8;

		private const int PROCESS_VM_WRITE = 32;

		private const int PROCESS_VM_READ = 16;

		private const uint MEM_FREE = 65536u;

		private const uint MEM_COMMIT = 4096u;

		private const uint MEM_RESERVE = 8192u;

		private const uint PAGE_READONLY = 2u;

		private const uint PAGE_READWRITE = 4u;

		private const uint PAGE_WRITECOPY = 8u;

		private const uint PAGE_EXECUTE_READWRITE = 64u;

		private const uint PAGE_EXECUTE_WRITECOPY = 128u;

		private const uint PAGE_EXECUTE = 16u;

		private const uint PAGE_EXECUTE_READ = 32u;

		private const uint PAGE_GUARD = 256u;

		private const uint PAGE_NOACCESS = 1u;

		private uint MEM_PRIVATE = 131072u;

		private uint MEM_IMAGE = 16777216u;

		/// <summary>
		/// The process handle that was opened. (Use OpenProcess function to populate this variable)
		/// </summary>
		public IntPtr pHandle;

		private Dictionary<string, CancellationTokenSource> FreezeTokenSrcs = new Dictionary<string, CancellationTokenSource>();

		public Process theProc = null;

		/// <summary>
		/// Check if opened process is 64bit. Used primarily for getCode().
		/// </summary>
		/// <returns>True if 64bit false if 32bit.</returns>
		private bool _is64Bit;

		/// <summary>
		/// Dictionary with our opened process module names with addresses.
		/// </summary>
		public Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

		private ProcessModule mainModule;

		public bool Is64Bit
		{
			get
			{
				return _is64Bit;
			}
			private set
			{
				_is64Bit = value;
			}
		}

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
		public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION32 lpBuffer, UIntPtr dwLength);

		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
		public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, UIntPtr dwLength);

		[DllImport("kernel32.dll")]
		private static extern uint GetLastError();

		public UIntPtr VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer)
		{
			checked
			{
				UIntPtr result;
				if (Is64Bit || IntPtr.Size == 8)
				{
					MEMORY_BASIC_INFORMATION64 lpBuffer2 = default(MEMORY_BASIC_INFORMATION64);
					result = Native_VirtualQueryEx(hProcess, lpAddress, out lpBuffer2, new UIntPtr((uint)Marshal.SizeOf(lpBuffer2)));
					lpBuffer.BaseAddress = lpBuffer2.BaseAddress;
					lpBuffer.AllocationBase = lpBuffer2.AllocationBase;
					lpBuffer.AllocationProtect = lpBuffer2.AllocationProtect;
					lpBuffer.RegionSize = (long)lpBuffer2.RegionSize;
					lpBuffer.State = lpBuffer2.State;
					lpBuffer.Protect = lpBuffer2.Protect;
					lpBuffer.Type = lpBuffer2.Type;
					return result;
				}
				MEMORY_BASIC_INFORMATION32 lpBuffer3 = default(MEMORY_BASIC_INFORMATION32);
				result = Native_VirtualQueryEx(hProcess, lpAddress, out lpBuffer3, new UIntPtr((uint)Marshal.SizeOf(lpBuffer3)));
				lpBuffer.BaseAddress = lpBuffer3.BaseAddress;
				lpBuffer.AllocationBase = lpBuffer3.AllocationBase;
				lpBuffer.AllocationProtect = lpBuffer3.AllocationProtect;
				lpBuffer.RegionSize = lpBuffer3.RegionSize;
				lpBuffer.State = lpBuffer3.State;
				lpBuffer.Protect = lpBuffer3.Protect;
				lpBuffer.Type = lpBuffer3.Type;
				return result;
			}
		}

		[DllImport("kernel32.dll")]
		private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

		[DllImport("kernel32.dll")]
		private static extern uint SuspendThread(IntPtr hThread);

		[DllImport("kernel32.dll")]
		private static extern int ResumeThread(IntPtr hThread);

		[DllImport("dbghelp.dll")]
		private static extern bool MiniDumpWriteDump(IntPtr hProcess, int ProcessId, IntPtr hFile, MINIDUMP_TYPE DumpType, IntPtr ExceptionParam, IntPtr UserStreamParam, IntPtr CallackParam);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);

		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, string lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll")]
		private static extern int GetProcessId(IntPtr handle);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern bool VirtualFreeEx(IntPtr hProcess, UIntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

		[DllImport("psapi.dll")]
		private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

		[DllImport("psapi.dll", SetLastError = true)]
		public static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);

		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern UIntPtr VirtualAllocEx(IntPtr hProcess, UIntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		[DllImport("kernel32.dll")]
		private static extern bool VirtualProtectEx(IntPtr hProcess, UIntPtr lpAddress, IntPtr dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
		public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
		private static extern bool _CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
		internal static extern int WaitForSingleObject(IntPtr handle, int milliseconds);

		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

		[DllImport("kernel32")]
		private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, UIntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

		[DllImport("kernel32")]
		private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CreateToolhelp32Snapshot([In] uint dwFlags, [In] uint th32ProcessID);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool Process32First([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("kernel32.dll")]
		private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

		[DllImport("kernel32.dll")]
		private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool Process32Next([In] IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		private bool IsDigitsOnly(string str)
		{
			foreach (char c in str)
			{
				if (c < '0' || c > '9')
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Freeze a value to an address.
		/// </summary>
		/// <param name="address">Your address</param>
		/// <param name="type">byte, 2bytes, bytes, float, int, string, double or long.</param>
		/// <param name="value">Value to freeze</param>
		/// <param name="file">ini file to read address from (OPTIONAL)</param>
		public void FreezeValue(string address, string type, string value, string file = "")
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			if (FreezeTokenSrcs.ContainsKey(address))
			{
				Debug.WriteLine("Changing Freezing Address " + address + " Value " + value);
				try
				{
					FreezeTokenSrcs[address].Cancel();
					FreezeTokenSrcs.Remove(address);
				}
				catch
				{
					Debug.WriteLine("ERROR: Avoided a crash. Address " + address + " was not frozen.");
				}
			}
			else
			{
				Debug.WriteLine("Adding Freezing Address " + address + " Value " + value);
			}
			FreezeTokenSrcs.Add(address, cts);
			Task.Factory.StartNew(delegate
			{
				while (!cts.Token.IsCancellationRequested)
				{
					WriteMemory(address, type, value, file);
					Thread.Sleep(25);
				}
			}, cts.Token);
		}

		/// <summary>
		/// Unfreeze a frozen value at an address
		/// </summary>
		/// <param name="address">address where frozen value is stored</param>
		public void UnfreezeValue(string address)
		{
			Debug.WriteLine("Un-Freezing Address " + address);
			try
			{
				FreezeTokenSrcs[address].Cancel();
				FreezeTokenSrcs.Remove(address);
			}
			catch
			{
				Debug.WriteLine("ERROR: Address " + address + " was not frozen.");
			}
		}

		/// <summary>
		/// Open the PC game process with all security and access rights.
		/// </summary>
		/// <param name="pid">Use process name or process ID here.</param>
		/// <returns></returns>
		public bool OpenProcess(int pid)
		{
			if (!IsAdmin())
			{
				Debug.WriteLine("WARNING: This program may not be running with raised privileges! Visit https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges");
			}
			if (pid <= 0)
			{
				Debug.WriteLine("ERROR: OpenProcess given proc ID 0.");
				return false;
			}
			if (theProc != null && theProc.Id == pid)
			{
				return true;
			}
			try
			{
				theProc = Process.GetProcessById(pid);
				if (theProc != null && !theProc.Responding)
				{
					Debug.WriteLine("ERROR: OpenProcess: Process is not responding or null.");
					return false;
				}
				pHandle = OpenProcess(2035711u, true, pid);
				try
				{
					Process.EnterDebugMode();
				}
				catch (Win32Exception)
				{
				}
				if (pHandle == IntPtr.Zero)
				{
					Debug.WriteLine("ERROR: OpenProcess has failed opening a handle to the target process (GetLastWin32ErrorCode: " + Marshal.GetLastWin32Error() + ")");
					Process.LeaveDebugMode();
					theProc = null;
					return false;
				}
				Is64Bit = Environment.Is64BitOperatingSystem && IsWow64Process(pHandle, out var lpSystemInfo) && !lpSystemInfo;
				mainModule = theProc.MainModule;
				GetModules();
				Debug.WriteLine("Process #" + theProc?.ToString() + " is now open.");
				return true;
			}
			catch (Exception ex2)
			{
				Debug.WriteLine("ERROR: OpenProcess has crashed. " + ex2);
				return false;
			}
		}

		/// <summary>
		/// Open the PC game process with all security and access rights.
		/// </summary>
		/// <param name="proc">Use process name or process ID here.</param>
		/// <returns></returns>
		public bool OpenProcess(string proc)
		{
			return OpenProcess(GetProcIdFromName(proc));
		}

		/// <summary>
		/// Check if program is running with administrative privileges. Read about it here: https://github.com/erfg12/memory.dll/wiki/Administrative-Privileges
		/// </summary>
		/// <returns></returns>
		public bool IsAdmin()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			try
			{
				WindowsIdentity current = WindowsIdentity.GetCurrent();
				try
				{
					WindowsPrincipal val = new WindowsPrincipal(current);
					return val.IsInRole((WindowsBuiltInRole)544);
				}
				finally
				{
					((IDisposable)current)?.Dispose();
				}
			}
			catch
			{
				Debug.WriteLine("ERROR: Could not determin if program is running as admin. Is the NuGet package \"System.Security.Principal.Windows\" missing?");
				return false;
			}
		}

		/// <summary>
		/// Builds the process modules dictionary (names with addresses).
		/// </summary>
		public void GetModules()
		{
			if (theProc == null)
			{
				return;
			}
			if (_is64Bit && IntPtr.Size != 8)
			{
				Debug.WriteLine("WARNING: Game is x64, but your Trainer is x86! You will be missing some modules, change your Trainer's Solution Platform.");
			}
			else if (!_is64Bit && IntPtr.Size == 8)
			{
				Debug.WriteLine("WARNING: Game is x86, but your Trainer is x64! You will be missing some modules, change your Trainer's Solution Platform.");
			}
			modules.Clear();
			foreach (ProcessModule module in theProc.Modules)
			{
				modules.Add(module.ModuleName, module.BaseAddress);
			}
			Debug.WriteLine("Found " + modules.Count() + " process modules.");
		}

		public void SetFocus()
		{
			SetForegroundWindow(theProc.MainWindowHandle);
		}

		/// <summary>
		/// Get the process ID number by process name.
		/// </summary>
		/// <param name="name">Example: "eqgame". Use task manager to find the name. Do not include .exe</param>
		/// <returns></returns>
		public int GetProcIdFromName(string name)
		{
			Process[] processes = Process.GetProcesses();
			if (name.ToLower().Contains(".exe"))
			{
				name = name.Replace(".exe", "");
			}
			if (name.ToLower().Contains(".bin"))
			{
				name = name.Replace(".bin", "");
			}
			Process[] array = processes;
			foreach (Process process in array)
			{
				if (process.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
				{
					return process.Id;
				}
			}
			return 0;
		}

		/// <summary>
		/// Get code from ini file.
		/// </summary>
		/// <param name="name">label for address or code</param>
		/// <param name="file">path and name of ini file</param>
		/// <returns></returns>
		public string LoadCode(string name, string file)
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			if (file != "")
			{
				uint privateProfileString = GetPrivateProfileString("codes", name, "", stringBuilder, checked((uint)stringBuilder.Capacity), file);
			}
			else
			{
				stringBuilder.Append(name);
			}
			return stringBuilder.ToString();
		}

		private int LoadIntCode(string name, string path)
		{
			try
			{
				int num = Convert.ToInt32(LoadCode(name, path), 16);
				if (num >= 0)
				{
					return num;
				}
				return 0;
			}
			catch
			{
				Debug.WriteLine("ERROR: LoadIntCode function crashed!");
				return 0;
			}
		}

		/// <summary>
		/// Make a named pipe (if not already made) and call to a remote function.
		/// </summary>
		/// <param name="func">remote function to call</param>
		/// <param name="name">name of the thread</param>
		public void ThreadStartClient(string func, string name)
		{
			using (NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(name))
			{
				if (!namedPipeClientStream.IsConnected)
				{
					namedPipeClientStream.Connect();
				}
				using (StreamWriter streamWriter = new StreamWriter(namedPipeClientStream))
				{
					if (!streamWriter.AutoFlush)
					{
						streamWriter.AutoFlush = true;
					}
					streamWriter.WriteLine(func);
				}
			}
		}

		/// <summary>
		/// Cut a string that goes on for too long or one that is possibly merged with another string.
		/// </summary>
		/// <param name="str">The string you want to cut.</param>
		/// <returns></returns>
		public string CutString(string str)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in str)
			{
				if (c >= ' ' && c <= '~')
				{
					stringBuilder.Append(c);
					continue;
				}
				break;
			}
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Clean up a string that has bad characters in it.
		/// </summary>
		/// <param name="str">The string you want to sanitize.</param>
		/// <returns></returns>
		public string SanitizeString(string str)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in str)
			{
				if (c >= ' ' && c <= '~')
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		public bool ChangeProtection(string code, MemoryProtection newProtection, out MemoryProtection oldProtection, string file = "")
		{
			UIntPtr code2 = GetCode(code, file);
			if (code2 == UIntPtr.Zero || pHandle == IntPtr.Zero)
			{
				oldProtection = (MemoryProtection)0u;
				return false;
			}
			return VirtualProtectEx(pHandle, code2, (IntPtr)(Is64Bit ? 8 : 4), newProtection, out oldProtection);
		}

		/// <summary>
		/// Reads up to `length ` bytes from an address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="length">The maximum bytes to read.</param>
		/// <param name="file">path and name of ini file.</param>
		/// <returns>The bytes read or null</returns>
		public byte[] ReadBytes(string code, long length, string file = "")
		{
			byte[] array = new byte[length];
			UIntPtr code2 = GetCode(code, file);
			if (!ReadProcessMemory(pHandle, code2, array, (UIntPtr)checked((ulong)length), IntPtr.Zero))
			{
				return null;
			}
			return array;
		}

		/// <summary>
		/// Read a float value from an address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		/// <param name="round">Round the value to 2 decimal places</param>
		/// <returns></returns>
		public float ReadFloat(string code, string file = "", bool round = true)
		{
			byte[] array = new byte[4];
			UIntPtr code2 = GetCode(code, file);
			try
			{
				if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)4uL, IntPtr.Zero))
				{
					float num = BitConverter.ToSingle(array, 0);
					float result = num;
					if (round)
					{
						result = (float)Math.Round(num, 2);
					}
					return result;
				}
				return 0f;
			}
			catch
			{
				return 0f;
			}
		}

		/// <summary>
		/// Read a string value from an address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		/// <param name="length">length of bytes to read (OPTIONAL)</param>
		/// <param name="zeroTerminated">terminate string at null char</param>
		/// <returns></returns>
		public string ReadString(string code, string file = "", int length = 32, bool zeroTerminated = true)
		{
			byte[] array = new byte[length];
			UIntPtr code2 = GetCode(code, file);
			if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)checked((ulong)length), IntPtr.Zero))
			{
				return zeroTerminated ? Encoding.UTF8.GetString(array).Split(new char[1])[0] : Encoding.UTF8.GetString(array);
			}
			return "";
		}

		/// <summary>
		/// Read a double value
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		/// <param name="round">Round the value to 2 decimal places</param>
		/// <returns></returns>
		public double ReadDouble(string code, string file = "", bool round = true)
		{
			byte[] array = new byte[8];
			UIntPtr code2 = GetCode(code, file);
			try
			{
				if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)8uL, IntPtr.Zero))
				{
					double num = BitConverter.ToDouble(array, 0);
					double result = num;
					if (round)
					{
						result = Math.Round(num, 2);
					}
					return result;
				}
				return 0.0;
			}
			catch
			{
				return 0.0;
			}
		}

		public int ReadUIntPtr(UIntPtr code)
		{
			byte[] array = new byte[4];
			if (ReadProcessMemory(pHandle, code, array, (UIntPtr)4uL, IntPtr.Zero))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		/// <summary>
		/// Read an integer from an address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		/// <returns></returns>
		public int ReadInt(string code, string file = "")
		{
			byte[] array = new byte[4];
			UIntPtr code2 = GetCode(code, file);
			if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)4uL, IntPtr.Zero))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		/// <summary>
		/// Read a long value from an address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		/// <returns></returns>
		public long ReadLong(string code, string file = "")
		{
			byte[] array = new byte[16];
			UIntPtr code2 = GetCode(code, file);
			if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)16uL, IntPtr.Zero))
			{
				return BitConverter.ToInt64(array, 0);
			}
			return 0L;
		}

		/// <summary>
		/// Read a UInt value from address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		/// <returns></returns>
		public uint ReadUInt(string code, string file = "")
		{
			byte[] array = new byte[4];
			UIntPtr code2 = GetCode(code, file);
			if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)4uL, IntPtr.Zero))
			{
				return BitConverter.ToUInt32(array, 0);
			}
			return 0u;
		}

		/// <summary>
		/// Reads a 2 byte value from an address and moves the address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="moveQty">Quantity to move.</param>
		/// <param name="file">path and name of ini file (OPTIONAL)</param>
		/// <returns></returns>
		public int Read2ByteMove(string code, int moveQty, string file = "")
		{
			byte[] array = new byte[4];
			UIntPtr code2 = GetCode(code, file);
			UIntPtr lpBaseAddress = UIntPtr.Add(code2, moveQty);
			if (ReadProcessMemory(pHandle, lpBaseAddress, array, (UIntPtr)2uL, IntPtr.Zero))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		/// <summary>
		/// Reads an integer value from address and moves the address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="moveQty">Quantity to move.</param>
		/// <param name="file">path and name of ini file (OPTIONAL)</param>
		/// <returns></returns>
		public int ReadIntMove(string code, int moveQty, string file = "")
		{
			byte[] array = new byte[4];
			UIntPtr code2 = GetCode(code, file);
			UIntPtr lpBaseAddress = UIntPtr.Add(code2, moveQty);
			if (ReadProcessMemory(pHandle, lpBaseAddress, array, (UIntPtr)4uL, IntPtr.Zero))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		/// <summary>
		/// Get UInt and move to another address by moveQty. Use in a for loop.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="moveQty">Quantity to move.</param>
		/// <param name="file">path and name of ini file (OPTIONAL)</param>
		/// <returns></returns>
		public ulong ReadUIntMove(string code, int moveQty, string file = "")
		{
			byte[] array = new byte[8];
			UIntPtr code2 = GetCode(code, file);
			UIntPtr lpBaseAddress = UIntPtr.Add(code2, moveQty);
			if (ReadProcessMemory(pHandle, lpBaseAddress, array, (UIntPtr)8uL, IntPtr.Zero))
			{
				return BitConverter.ToUInt64(array, 0);
			}
			return 0uL;
		}

		/// <summary>
		/// Read a 2 byte value from an address. Returns an integer.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and file name to ini file. (OPTIONAL)</param>
		/// <returns></returns>
		public int Read2Byte(string code, string file = "")
		{
			byte[] array = new byte[4];
			UIntPtr code2 = GetCode(code, file);
			if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)2uL, IntPtr.Zero))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		/// <summary>
		/// Read 1 byte from address.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and file name of ini file. (OPTIONAL)</param>
		/// <returns></returns>
		public int ReadByte(string code, string file = "")
		{
			byte[] array = new byte[1];
			UIntPtr code2 = GetCode(code, file);
			if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)1uL, IntPtr.Zero))
			{
				return array[0];
			}
			return 0;
		}

		/// <summary>
		/// Reads a byte from memory and splits it into bits
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="file">path and file name of ini file. (OPTIONAL)</param>
		/// <returns>Array of 8 booleans representing each bit of the byte read</returns>
		public bool[] ReadBits(string code, string file = "")
		{
			byte[] array = new byte[1];
			UIntPtr code2 = GetCode(code, file);
			bool[] array2 = new bool[8];
			if (!ReadProcessMemory(pHandle, code2, array, (UIntPtr)1uL, IntPtr.Zero))
			{
				return array2;
			}
			if (!BitConverter.IsLittleEndian)
			{
				throw new Exception("Should be little endian");
			}
			for (int i = 0; i < 8; i = checked(i + 1))
			{
				array2[i] = Convert.ToBoolean(array[0] & (1 << i));
			}
			return array2;
		}

		public int ReadPByte(UIntPtr address, string code, string file = "")
		{
			byte[] array = new byte[4];
			if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), array, (UIntPtr)1uL, IntPtr.Zero))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		public float ReadPFloat(UIntPtr address, string code, string file = "")
		{
			byte[] array = new byte[4];
			if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), array, (UIntPtr)4uL, IntPtr.Zero))
			{
				float num = BitConverter.ToSingle(array, 0);
				return (float)Math.Round(num, 2);
			}
			return 0f;
		}

		public int ReadPInt(UIntPtr address, string code, string file = "")
		{
			byte[] array = new byte[4];
			if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), array, (UIntPtr)4uL, IntPtr.Zero))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		public string ReadPString(UIntPtr address, string code, string file = "")
		{
			byte[] array = new byte[32];
			if (ReadProcessMemory(pHandle, address + LoadIntCode(code, file), array, (UIntPtr)32uL, IntPtr.Zero))
			{
				return CutString(Encoding.ASCII.GetString(array));
			}
			return "";
		}

		/// <summary>
		/// Write to memory address. See https://github.com/erfg12/memory.dll/wiki/writeMemory() for more information.
		/// </summary>
		/// <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		/// <param name="type">byte, 2bytes, bytes, float, int, string, double or long.</param>
		/// <param name="write">value to write to address.</param>
		/// <param name="file">path and name of .ini file (OPTIONAL)</param>
		/// <param name="stringEncoding">System.Text.Encoding.UTF8 (DEFAULT). Other options: ascii, unicode, utf32, utf7</param>
		public bool WriteMemory(string code, string type, string write, string file = "", Encoding stringEncoding = null)
		{
			byte[] array = new byte[4];
			int num = 4;
			UIntPtr code2 = GetCode(code, file);
			checked
			{
				if (type.ToLower() == "float")
				{
					array = BitConverter.GetBytes(Convert.ToSingle(write));
					num = 4;
				}
				else if (type.ToLower() == "int")
				{
					array = BitConverter.GetBytes(Convert.ToInt32(write));
					num = 4;
				}
				else if (type.ToLower() == "byte")
				{
					array = new byte[1]
					{
						Convert.ToByte(write, 16)
					};
					num = 1;
				}
				else if (type.ToLower() == "2bytes")
				{
					array = new byte[2]
					{
						(byte)unchecked(Convert.ToInt32(write) % 256),
						(byte)unchecked(Convert.ToInt32(write) / 256)
					};
					num = 2;
				}
				else if (type.ToLower() == "bytes")
				{
					if (write.Contains(",") || write.Contains(" "))
					{
						string[] array2 = ((!write.Contains(",")) ? write.Split(new char[1]
						{
							' '
						}) : write.Split(new char[1]
						{
							','
						}));
						int num2 = array2.Count();
						array = new byte[num2];
						for (int i = 0; i < num2; i++)
						{
							array[i] = Convert.ToByte(array2[i], 16);
						}
						num = array2.Count();
					}
					else
					{
						array = new byte[1]
						{
							Convert.ToByte(write, 16)
						};
						num = 1;
					}
				}
				else if (type.ToLower() == "double")
				{
					array = BitConverter.GetBytes(Convert.ToDouble(write));
					num = 8;
				}
				else if (type.ToLower() == "long")
				{
					array = BitConverter.GetBytes(Convert.ToInt64(write));
					num = 8;
				}
				else if (type.ToLower() == "string")
				{
					array = ((stringEncoding != null) ? stringEncoding.GetBytes(write) : Encoding.UTF8.GetBytes(write));
					num = array.Length;
				}
				bool flag = false;
				MemoryProtection oldProtection = (MemoryProtection)0u;
				ChangeProtection(code, MemoryProtection.ExecuteReadWrite, out oldProtection);
				flag = WriteProcessMemory(pHandle, code2, array, (UIntPtr)(ulong)num, IntPtr.Zero);
				ChangeProtection(code, oldProtection, out var _);
				return flag;
			}
		}

		/// <summary>
		/// Write to address and move by moveQty. Good for byte arrays. See https://github.com/erfg12/memory.dll/wiki/Writing-a-Byte-Array for more information.
		/// </summary>
		///             <param name="code">address, module + pointer + offset, module + offset OR label in .ini file.</param>
		///             <param name="type">byte, bytes, float, int, string or long.</param>
		/// <param name="write">byte to write</param>
		/// <param name="moveQty">quantity to move</param>
		/// <param name="file">path and name of .ini file (OPTIONAL)</param>
		/// <returns></returns>
		public bool WriteMove(string code, string type, string write, int moveQty, string file = "")
		{
			byte[] array = new byte[4];
			int num = 4;
			UIntPtr code2 = GetCode(code, file);
			switch (type)
			{
			case "float":
				array = new byte[write.Length];
				array = BitConverter.GetBytes(Convert.ToSingle(write));
				num = write.Length;
				break;
			case "int":
				array = BitConverter.GetBytes(Convert.ToInt32(write));
				num = 4;
				break;
			case "double":
				array = BitConverter.GetBytes(Convert.ToDouble(write));
				num = 8;
				break;
			case "long":
				array = BitConverter.GetBytes(Convert.ToInt64(write));
				num = 8;
				break;
			case "byte":
				array = new byte[1]
				{
					Convert.ToByte(write, 16)
				};
				num = 1;
				break;
			case "string":
				array = new byte[write.Length];
				array = Encoding.UTF8.GetBytes(write);
				num = write.Length;
				break;
			}
			UIntPtr lpBaseAddress = UIntPtr.Add(code2, moveQty);
			Debug.Write("DEBUG: Writing bytes [TYPE:" + type + " ADDR:[O]" + code2 + " [N]" + lpBaseAddress + " MQTY:" + moveQty + "] " + string.Join(",", array) + Environment.NewLine);
			Thread.Sleep(1000);
			return WriteProcessMemory(pHandle, lpBaseAddress, array, (UIntPtr)checked((ulong)num), IntPtr.Zero);
		}

		/// <summary>
		/// Write byte array to addresses.
		/// </summary>
		/// <param name="code">address to write to</param>
		/// <param name="write">byte array to write</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		public void WriteBytes(string code, byte[] write, string file = "")
		{
			UIntPtr code2 = GetCode(code, file);
			WriteProcessMemory(pHandle, code2, write, (UIntPtr)checked((ulong)write.Length), IntPtr.Zero);
		}

		/// <summary>
		/// Takes an array of 8 booleans and writes to a single byte
		/// </summary>
		/// <param name="code">address to write to</param>
		/// <param name="bits">Array of 8 booleans to write</param>
		/// <param name="file">path and name of ini file. (OPTIONAL)</param>
		public void WriteBits(string code, bool[] bits, string file = "")
		{
			if (bits.Length != 8)
			{
				throw new ArgumentException("Not enough bits for a whole byte", "bits");
			}
			byte[] array = new byte[1];
			UIntPtr code2 = GetCode(code, file);
			checked
			{
				for (int i = 0; i < 8; i++)
				{
					if (bits[i])
					{
						array[0] |= (byte)(1 << i);
					}
				}
				WriteProcessMemory(pHandle, code2, array, (UIntPtr)1uL, IntPtr.Zero);
			}
		}

		/// <summary>
		/// Write byte array to address
		/// </summary>
		/// <param name="address">Address to write to</param>
		/// <param name="write">Byte array to write to</param>
		public void WriteBytes(UIntPtr address, byte[] write)
		{
			WriteProcessMemory(pHandle, address, write, (UIntPtr)checked((ulong)write.Length), out var _);
		}

		/// <summary>
		/// Convert code from string to real address. If path is not blank, will pull from ini file.
		/// </summary>
		/// <param name="name">label in ini file or code</param>
		/// <param name="path">path to ini file (OPTIONAL)</param>
		/// <param name="size">size of address (default is 8)</param>
		/// <returns></returns>
		public UIntPtr GetCode(string name, string path = "", int size = 8)
		{
			string text = "";
			if (Is64Bit)
			{
				if (size == 8)
				{
					size = 16;
				}
				return Get64BitCode(name, path, size);
			}
			text = ((!(path != "")) ? name : LoadCode(name, path));
			if (text == "")
			{
				return UIntPtr.Zero;
			}
			if (text.Contains(" "))
			{
				text.Replace(" ", string.Empty);
			}
			if (!text.Contains("+") && !text.Contains(","))
			{
				return new UIntPtr(Convert.ToUInt32(text, 16));
			}
			string text2 = text;
			checked
			{
				if (text.Contains("+"))
				{
					text2 = text.Substring(text.IndexOf('+') + 1);
				}
				byte[] array = new byte[size];
				if (Enumerable.Contains(text2, ','))
				{
					List<int> list = new List<int>();
					string[] array2 = text2.Split(new char[1]
					{
						','
					});
					string[] array3 = array2;
					foreach (string text3 in array3)
					{
						string text4 = text3;
						if (text3.Contains("0x"))
						{
							text4 = text3.Replace("0x", "");
						}
						int num = 0;
						if (!text3.Contains("-"))
						{
							num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
						}
						else
						{
							text4 = text4.Replace("-", "");
							num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
							num *= -1;
						}
						list.Add(num);
					}
					int[] array4 = list.ToArray();
					if (text.Contains("base") || text.Contains("main"))
					{
						ReadProcessMemory(pHandle, (UIntPtr)(ulong)((int)mainModule.BaseAddress + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
					}
					else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+"))
					{
						string[] array5 = text.Split(new char[1]
						{
							'+'
						});
						IntPtr value = IntPtr.Zero;
						if (!array5[0].ToLower().Contains(".dll") && !array5[0].ToLower().Contains(".exe") && !array5[0].ToLower().Contains(".bin"))
						{
							string text5 = array5[0];
							if (text5.Contains("0x"))
							{
								text5 = text5.Replace("0x", "");
							}
							value = (IntPtr)int.Parse(text5, NumberStyles.HexNumber);
						}
						else
						{
							try
							{
								value = modules[array5[0]];
							}
							catch
							{
								Debug.WriteLine("Module " + array5[0] + " was not found in module list!");
								Debug.WriteLine("Modules: " + string.Join(",", modules));
							}
						}
						ReadProcessMemory(pHandle, (UIntPtr)(ulong)((int)value + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
					}
					else
					{
						ReadProcessMemory(pHandle, (UIntPtr)(ulong)array4[0], array, (UIntPtr)(ulong)size, IntPtr.Zero);
					}
					uint num2 = BitConverter.ToUInt32(array, 0);
					UIntPtr uIntPtr = (UIntPtr)0uL;
					for (int j = 1; j < array4.Length; j++)
					{
						uIntPtr = new UIntPtr(Convert.ToUInt32(unchecked((long)num2) + unchecked((long)array4[j])));
						ReadProcessMemory(pHandle, uIntPtr, array, (UIntPtr)(ulong)size, IntPtr.Zero);
						num2 = BitConverter.ToUInt32(array, 0);
					}
					return uIntPtr;
				}
				int num3 = Convert.ToInt32(text2, 16);
				IntPtr value2 = IntPtr.Zero;
				if (text.ToLower().Contains("base") || text.ToLower().Contains("main"))
				{
					value2 = mainModule.BaseAddress;
				}
				else if (!text.ToLower().Contains("base") && !text.ToLower().Contains("main") && text.Contains("+"))
				{
					string[] array6 = text.Split(new char[1]
					{
						'+'
					});
					if (!array6[0].ToLower().Contains(".dll") && !array6[0].ToLower().Contains(".exe") && !array6[0].ToLower().Contains(".bin"))
					{
						string text6 = array6[0];
						if (text6.Contains("0x"))
						{
							text6 = text6.Replace("0x", "");
						}
						value2 = (IntPtr)int.Parse(text6, NumberStyles.HexNumber);
					}
					else
					{
						try
						{
							value2 = modules[array6[0]];
						}
						catch
						{
							Debug.WriteLine("Module " + array6[0] + " was not found in module list!");
							Debug.WriteLine("Modules: " + string.Join(",", modules));
						}
					}
				}
				else
				{
					value2 = modules[text.Split(new char[1]
					{
						'+'
					})[0]];
				}
				return (UIntPtr)(ulong)((int)value2 + num3);
			}
		}

		/// <summary>
		/// Convert code from string to real address. If path is not blank, will pull from ini file.
		/// </summary>
		/// <param name="name">label in ini file OR code</param>
		/// <param name="path">path to ini file (OPTIONAL)</param>
		/// <param name="size">size of address (default is 16)</param>
		/// <returns></returns>
		public UIntPtr Get64BitCode(string name, string path = "", int size = 16)
		{
			string text = "";
			text = ((!(path != "")) ? name : LoadCode(name, path));
			if (text == "")
			{
				return UIntPtr.Zero;
			}
			if (text.Contains(" "))
			{
				text.Replace(" ", string.Empty);
			}
			string text2 = text;
			checked
			{
				if (text.Contains("+"))
				{
					text2 = text.Substring(text.IndexOf('+') + 1);
				}
				byte[] array = new byte[size];
				if (!text.Contains("+") && !text.Contains(","))
				{
					return new UIntPtr(Convert.ToUInt64(text, 16));
				}
				if (Enumerable.Contains(text2, ','))
				{
					List<long> list = new List<long>();
					string[] array2 = text2.Split(new char[1]
					{
						','
					});
					string[] array3 = array2;
					foreach (string text3 in array3)
					{
						string text4 = text3;
						if (text3.Contains("0x"))
						{
							text4 = text3.Replace("0x", "");
						}
						long num = 0L;
						if (!text3.Contains("-"))
						{
							num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
						}
						else
						{
							text4 = text4.Replace("-", "");
							num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
							num *= -1;
						}
						list.Add(num);
					}
					long[] array4 = list.ToArray();
					if (text.Contains("base") || text.Contains("main"))
					{
						ReadProcessMemory(pHandle, (UIntPtr)(ulong)((long)mainModule.BaseAddress + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
					}
					else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+"))
					{
						string[] array5 = text.Split(new char[1]
						{
							'+'
						});
						IntPtr value = IntPtr.Zero;
						if (!array5[0].ToLower().Contains(".dll") && !array5[0].ToLower().Contains(".exe") && !array5[0].ToLower().Contains(".bin"))
						{
							value = (IntPtr)long.Parse(array5[0], NumberStyles.HexNumber);
						}
						else
						{
							try
							{
								value = modules[array5[0]];
							}
							catch
							{
								Debug.WriteLine("Module " + array5[0] + " was not found in module list!");
								Debug.WriteLine("Modules: " + string.Join(",", modules));
							}
						}
						ReadProcessMemory(pHandle, (UIntPtr)(ulong)((long)value + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
					}
					else
					{
						ReadProcessMemory(pHandle, (UIntPtr)(ulong)array4[0], array, (UIntPtr)(ulong)size, IntPtr.Zero);
					}
					long num2 = BitConverter.ToInt64(array, 0);
					UIntPtr uIntPtr = (UIntPtr)0uL;
					for (int j = 1; j < array4.Length; j++)
					{
						uIntPtr = new UIntPtr(Convert.ToUInt64(num2 + array4[j]));
						ReadProcessMemory(pHandle, uIntPtr, array, (UIntPtr)(ulong)size, IntPtr.Zero);
						num2 = BitConverter.ToInt64(array, 0);
					}
					return uIntPtr;
				}
				long num3 = Convert.ToInt64(text2, 16);
				IntPtr value2 = IntPtr.Zero;
				if (text.Contains("base") || text.Contains("main"))
				{
					value2 = mainModule.BaseAddress;
				}
				else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+"))
				{
					string[] array6 = text.Split(new char[1]
					{
						'+'
					});
					if (!array6[0].ToLower().Contains(".dll") && !array6[0].ToLower().Contains(".exe") && !array6[0].ToLower().Contains(".bin"))
					{
						string text5 = array6[0];
						if (text5.Contains("0x"))
						{
							text5 = text5.Replace("0x", "");
						}
						value2 = (IntPtr)long.Parse(text5, NumberStyles.HexNumber);
					}
					else
					{
						try
						{
							value2 = modules[array6[0]];
						}
						catch
						{
							Debug.WriteLine("Module " + array6[0] + " was not found in module list!");
							Debug.WriteLine("Modules: " + string.Join(",", modules));
						}
					}
				}
				else
				{
					value2 = modules[text.Split(new char[1]
					{
						'+'
					})[0]];
				}
				return (UIntPtr)(ulong)((long)value2 + num3);
			}
		}

		/// <summary>
		/// Close the process when finished.
		/// </summary>
		public void CloseProcess()
		{
			_ = pHandle;
			if (0 == 0)
			{
				CloseHandle(pHandle);
				theProc = null;
			}
		}

		/// <summary>
		/// Inject a DLL file.
		/// </summary>
		/// <param name="strDllName">path and name of DLL file.</param>
		public unsafe bool InjectDll(string strDllName)
		{
			foreach (ProcessModule module in theProc.Modules)
			{
				if (module.ModuleName.StartsWith("inject", StringComparison.InvariantCultureIgnoreCase))
				{
					return false;
				}
			}
			if (!theProc.Responding)
			{
				return false;
			}
			UIntPtr uIntPtr;
			IntPtr lpNumberOfBytesWritten;
			UIntPtr procAddress;
			checked
			{
				int num = strDllName.Length + 1;
				uIntPtr = VirtualAllocEx(pHandle, (UIntPtr)unchecked((void*)null), (uint)num, 12288u, 4u);
				WriteProcessMemory(pHandle, uIntPtr, strDllName, (UIntPtr)(ulong)num, out lpNumberOfBytesWritten);
				procAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
				bool flag = false;
			}
			IntPtr intPtr = CreateRemoteThread(pHandle, (IntPtr)(void*)null, 0u, procAddress, uIntPtr, 0u, out lpNumberOfBytesWritten);
			bool flag2 = false;
			int num2 = WaitForSingleObject(intPtr, 10000);
			if ((long)num2 == 128 || (long)num2 == 258)
			{
				bool flag3 = true;
				CloseHandle(intPtr);
				return false;
			}
			VirtualFreeEx(pHandle, uIntPtr, (UIntPtr)0uL, 32768u);
			bool flag4 = true;
			CloseHandle(intPtr);
			return true;
		}

		/// <summary>
		/// Creates a code cave to write custom opcodes in target process
		/// </summary>
		/// <param name="code">Address to create the trampoline</param>
		/// <param name="newBytes">The opcodes to write in the code cave</param>
		/// <param name="replaceCount">The number of bytes being replaced</param>
		/// <param name="size">size of the allocated region</param>
		/// <param name="file">ini file to look in</param>
		/// <remarks>Please ensure that you use the proper replaceCount
		/// if you replace halfway in an instruction you may cause bad things</remarks>
		/// <returns>UIntPtr to created code cave for use for later deallocation</returns>
		public UIntPtr CreateCodeCave(string code, byte[] newBytes, int replaceCount, int size = 4096, string file = "")
		{
			if (replaceCount < 5)
			{
				return UIntPtr.Zero;
			}
			UIntPtr code2 = GetCode(code, file);
			UIntPtr uIntPtr = code2;
			UIntPtr uIntPtr2 = UIntPtr.Zero;
			UIntPtr uIntPtr3 = uIntPtr;
			checked
			{
				for (int i = 0; i < 10; i++)
				{
					if (!(uIntPtr2 == UIntPtr.Zero))
					{
						break;
					}
					uIntPtr2 = VirtualAllocEx(pHandle, FindFreeBlockForRegion(uIntPtr3, (uint)size), (uint)size, 12288u, 64u);
					if (uIntPtr2 == UIntPtr.Zero)
					{
						uIntPtr3 = UIntPtr.Add(uIntPtr3, 65536);
					}
				}
				if (uIntPtr2 == UIntPtr.Zero)
				{
					uIntPtr2 = VirtualAllocEx(pHandle, UIntPtr.Zero, (uint)size, 12288u, 64u);
				}
				int num = ((replaceCount > 5) ? (replaceCount - 5) : 0);
				int value = (int)((long)(ulong)uIntPtr2 - (long)(ulong)uIntPtr - 5);
				byte[] array = new byte[5 + num];
				array[0] = 233;
				BitConverter.GetBytes(value).CopyTo(array, 1);
				for (int j = 5; j < array.Length; j++)
				{
					array[j] = 144;
				}
				WriteBytes(uIntPtr, array);
				byte[] array2 = new byte[5 + newBytes.Length];
				value = (int)((long)(ulong)uIntPtr + array.Length - ((long)(ulong)uIntPtr2 + newBytes.Length) - 5);
				newBytes.CopyTo(array2, 0);
				array2[newBytes.Length] = 233;
				BitConverter.GetBytes(value).CopyTo(array2, newBytes.Length + 1);
				WriteBytes(uIntPtr2, array2);
				return uIntPtr2;
			}
		}

		private UIntPtr FindFreeBlockForRegion(UIntPtr baseAddress, uint size)
		{
			UIntPtr uIntPtr = UIntPtr.Subtract(baseAddress, 1879048192);
			UIntPtr value = UIntPtr.Add(baseAddress, 1879048192);
			UIntPtr uIntPtr2 = UIntPtr.Zero;
			UIntPtr zero = UIntPtr.Zero;
			GetSystemInfo(out var lpSystemInfo);
			checked
			{
				if (Is64Bit)
				{
					if ((long)(ulong)uIntPtr > (long)(ulong)lpSystemInfo.maximumApplicationAddress || (long)(ulong)uIntPtr < (long)(ulong)lpSystemInfo.minimumApplicationAddress)
					{
						uIntPtr = lpSystemInfo.minimumApplicationAddress;
					}
					if ((long)(ulong)value < (long)(ulong)lpSystemInfo.minimumApplicationAddress || (long)(ulong)value > (long)(ulong)lpSystemInfo.maximumApplicationAddress)
					{
						value = lpSystemInfo.maximumApplicationAddress;
					}
				}
				else
				{
					uIntPtr = lpSystemInfo.minimumApplicationAddress;
					value = lpSystemInfo.maximumApplicationAddress;
				}
				UIntPtr uIntPtr3 = uIntPtr;
				UIntPtr uIntPtr4 = uIntPtr3;
				MEMORY_BASIC_INFORMATION lpBuffer;
				while (VirtualQueryEx(pHandle, uIntPtr3, out lpBuffer).ToUInt64() != 0)
				{
					if ((long)(ulong)lpBuffer.BaseAddress > (long)(ulong)value)
					{
						return UIntPtr.Zero;
					}
					if (lpBuffer.State == 65536 && lpBuffer.RegionSize > size)
					{
						if (unchecked(checked((long)(ulong)lpBuffer.BaseAddress) % (long)lpSystemInfo.allocationGranularity) > 0)
						{
							zero = lpBuffer.BaseAddress;
							int num = (int)(unchecked((long)lpSystemInfo.allocationGranularity) - unchecked(checked((long)(ulong)zero) % (long)lpSystemInfo.allocationGranularity));
							if (lpBuffer.RegionSize - num >= size)
							{
								zero = UIntPtr.Add(zero, num);
								if ((long)(ulong)zero < (long)(ulong)baseAddress)
								{
									zero = UIntPtr.Add(zero, (int)(lpBuffer.RegionSize - num - unchecked((long)size)));
									if ((long)(ulong)zero > (long)(ulong)baseAddress)
									{
										zero = baseAddress;
									}
									zero = UIntPtr.Subtract(zero, (int)unchecked(checked((long)(ulong)zero) % (long)lpSystemInfo.allocationGranularity));
								}
								if (Math.Abs((long)(ulong)zero - (long)(ulong)baseAddress) < Math.Abs((long)(ulong)uIntPtr2 - (long)(ulong)baseAddress))
								{
									uIntPtr2 = zero;
								}
							}
						}
						else
						{
							zero = lpBuffer.BaseAddress;
							if ((long)(ulong)zero < (long)(ulong)baseAddress)
							{
								zero = UIntPtr.Add(zero, (int)(lpBuffer.RegionSize - unchecked((long)size)));
								if ((long)(ulong)zero > (long)(ulong)baseAddress)
								{
									zero = baseAddress;
								}
								zero = UIntPtr.Subtract(zero, (int)unchecked(checked((long)(ulong)zero) % (long)lpSystemInfo.allocationGranularity));
							}
							if (Math.Abs((long)(ulong)zero - (long)(ulong)baseAddress) < Math.Abs((long)(ulong)uIntPtr2 - (long)(ulong)baseAddress))
							{
								uIntPtr2 = zero;
							}
						}
					}
					if (unchecked(lpBuffer.RegionSize % (long)lpSystemInfo.allocationGranularity) > 0)
					{
						lpBuffer.RegionSize += unchecked((long)lpSystemInfo.allocationGranularity) - unchecked(lpBuffer.RegionSize % (long)lpSystemInfo.allocationGranularity);
					}
					uIntPtr4 = uIntPtr3;
					uIntPtr3 = UIntPtr.Add(lpBuffer.BaseAddress, (int)lpBuffer.RegionSize);
					if ((long)(ulong)uIntPtr3 > (long)(ulong)value)
					{
						return uIntPtr2;
					}
					if ((long)(ulong)uIntPtr4 > (long)(ulong)uIntPtr3)
					{
						return uIntPtr2;
					}
				}
				return uIntPtr2;
			}
		}

		public static void SuspendProcess(int pid)
		{
			Process processById = Process.GetProcessById(pid);
			if (processById.ProcessName == string.Empty)
			{
				return;
			}
			foreach (ProcessThread thread in processById.Threads)
			{
				IntPtr intPtr = OpenThread(ThreadAccess.SUSPEND_RESUME, false, checked((uint)thread.Id));
				if (!(intPtr == IntPtr.Zero))
				{
					SuspendThread(intPtr);
					CloseHandle(intPtr);
				}
			}
		}

		public static void ResumeProcess(int pid)
		{
			Process processById = Process.GetProcessById(pid);
			if (processById.ProcessName == string.Empty)
			{
				return;
			}
			foreach (ProcessThread thread in processById.Threads)
			{
				IntPtr intPtr = OpenThread(ThreadAccess.SUSPEND_RESUME, false, checked((uint)thread.Id));
				if (!(intPtr == IntPtr.Zero))
				{
					int num = 0;
					do
					{
						num = ResumeThread(intPtr);
					}
					while (num > 0);
					CloseHandle(intPtr);
				}
			}
		}

		private async Task PutTaskDelay(int delay)
		{
			await Task.Delay(delay);
		}

		private void AppendAllBytes(string path, byte[] bytes)
		{
			using (FileStream fileStream = new FileStream(path, FileMode.Append))
			{
				fileStream.Write(bytes, 0, bytes.Length);
			}
		}

		public byte[] FileToBytes(string path, bool dontDelete = false)
		{
			byte[] result = File.ReadAllBytes(path);
			if (!dontDelete)
			{
				File.Delete(path);
			}
			return result;
		}

		public string MSize()
		{
			if (Is64Bit)
			{
				return "x16";
			}
			return "x8";
		}

		/// <summary>
		/// Convert a byte array to hex values in a string.
		/// </summary>
		/// <param name="ba">your byte array to convert</param>
		/// <returns></returns>
		public static string ByteArrayToHexString(byte[] ba)
		{
			checked
			{
				StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
				int num = 1;
				foreach (byte b in ba)
				{
					if (num == 16)
					{
						stringBuilder.AppendFormat("{0:x2}{1}", b, Environment.NewLine);
						num = 0;
					}
					else
					{
						stringBuilder.AppendFormat("{0:x2} ", b);
					}
					num++;
				}
				return stringBuilder.ToString().ToUpper();
			}
		}

		public static string ByteArrayToString(byte[] ba)
		{
			StringBuilder stringBuilder = new StringBuilder(checked(ba.Length * 2));
			foreach (byte b in ba)
			{
				stringBuilder.AppendFormat("{0:x2} ", b);
			}
			return stringBuilder.ToString();
		}

		public ulong GetMinAddress()
		{
			GetSystemInfo(out var lpSystemInfo);
			return (ulong)lpSystemInfo.minimumApplicationAddress;
		}

		/// <summary>
		/// Dump memory page by page to a dump.dmp file. Can be used with Cheat Engine.
		/// </summary>
		public bool DumpMemory(string file = "dump.dmp")
		{
			Debug.Write("[DEBUG] memory dump starting... (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);
			SYSTEM_INFO lpSystemInfo = default(SYSTEM_INFO);
			GetSystemInfo(out lpSystemInfo);
			UIntPtr uIntPtr = lpSystemInfo.minimumApplicationAddress;
			UIntPtr maximumApplicationAddress = lpSystemInfo.maximumApplicationAddress;
			checked
			{
				long num = (long)(ulong)uIntPtr;
				long num2 = theProc.VirtualMemorySize64 + num;
				if (File.Exists(file))
				{
					File.Delete(file);
				}
				MEMORY_BASIC_INFORMATION lpBuffer = default(MEMORY_BASIC_INFORMATION);
				while (num < num2)
				{
					VirtualQueryEx(pHandle, uIntPtr, out lpBuffer);
					byte[] array = new byte[lpBuffer.RegionSize];
					UIntPtr nSize = (UIntPtr)(ulong)lpBuffer.RegionSize;
					UIntPtr lpBaseAddress = (UIntPtr)(ulong)(long)(ulong)lpBuffer.BaseAddress;
					ReadProcessMemory(pHandle, lpBaseAddress, array, nSize, IntPtr.Zero);
					AppendAllBytes(file, array);
					num += lpBuffer.RegionSize;
					uIntPtr = new UIntPtr((ulong)num);
				}
				Debug.Write("[DEBUG] memory dump completed. Saving dump file to " + file + ". (" + DateTime.Now.ToString("h:mm:ss tt") + ")" + Environment.NewLine);
				return true;
			}
		}

		/// <summary>
		/// Array of byte scan.
		/// </summary>
		/// <param name="search">array of bytes to search for, OR your ini code label.</param>
		/// <param name="writable">Include writable addresses in scan</param>
		/// <param name="executable">Include executable addresses in scan</param>
		/// <param name="file">ini file (OPTIONAL)</param>
		/// <returns>IEnumerable of all addresses found.</returns>
		public Task<IEnumerable<long>> AoBScan(string search, bool writable = false, bool executable = true, string file = "")
		{
			return AoBScan(0L, long.MaxValue, search, writable, executable, file);
		}

		/// <summary>
		/// Array of byte scan.
		/// </summary>
		/// <param name="search">array of bytes to search for, OR your ini code label.</param>
		/// <param name="readable">Include readable addresses in scan</param>
		/// <param name="writable">Include writable addresses in scan</param>
		/// <param name="executable">Include executable addresses in scan</param>
		/// <param name="file">ini file (OPTIONAL)</param>
		/// <returns>IEnumerable of all addresses found.</returns>
		public Task<IEnumerable<long>> AoBScan(string search, bool readable, bool writable, bool executable, string file = "")
		{
			return AoBScan(0L, long.MaxValue, search, readable, writable, executable, file);
		}

		/// <summary>
		/// Array of Byte scan.
		/// </summary>
		/// <param name="start">Your starting address.</param>
		/// <param name="end">ending address</param>
		/// <param name="search">array of bytes to search for, OR your ini code label.</param>
		/// <param name="file">ini file (OPTIONAL)</param>
		/// <param name="writable">Include writable addresses in scan</param>
		/// <param name="executable">Include executable addresses in scan</param>
		/// <returns>IEnumerable of all addresses found.</returns>
		public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool writable = false, bool executable = true, string file = "")
		{
			return AoBScan(start, end, search, false, writable, executable, file);
		}

		/// <summary>
		/// Array of Byte scan.
		/// </summary>
		/// <param name="start">Your starting address.</param>
		/// <param name="end">ending address</param>
		/// <param name="search">array of bytes to search for, OR your ini code label.</param>
		/// <param name="file">ini file (OPTIONAL)</param>
		/// <param name="readable">Include readable addresses in scan</param>
		/// <param name="writable">Include writable addresses in scan</param>
		/// <param name="executable">Include executable addresses in scan</param>
		/// <returns>IEnumerable of all addresses found.</returns>
		public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool readable, bool writable, bool executable, string file = "")
		{
			return Task.Run(checked(delegate
			{
				List<MemoryRegionResult> list = new List<MemoryRegionResult>();
				string text = LoadCode(search, file);
				string[] array = text.Split(new char[1]
				{
					' '
				});
				byte[] aobPattern = new byte[array.Length];
				byte[] mask = new byte[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					if (text2 == "??" || (text2.Length == 1 && text2 == "?"))
					{
						mask[i] = 0;
						array[i] = "0x00";
					}
					else if (char.IsLetterOrDigit(text2[0]) && text2[1] == '?')
					{
						mask[i] = 240;
						array[i] = text2[0] + "0";
					}
					else if (char.IsLetterOrDigit(text2[1]) && text2[0] == '?')
					{
						mask[i] = 15;
						array[i] = "0" + text2[1];
					}
					else
					{
						mask[i] = byte.MaxValue;
					}
				}
				for (int j = 0; j < array.Length; j++)
				{
					aobPattern[j] = (byte)(Convert.ToByte(array[j], 16) & mask[j]);
				}
				SYSTEM_INFO lpSystemInfo = default(SYSTEM_INFO);
				GetSystemInfo(out lpSystemInfo);
				UIntPtr minimumApplicationAddress = lpSystemInfo.minimumApplicationAddress;
				UIntPtr maximumApplicationAddress = lpSystemInfo.maximumApplicationAddress;
				if (start < (long)minimumApplicationAddress.ToUInt64())
				{
					start = (long)minimumApplicationAddress.ToUInt64();
				}
				if (end > (long)maximumApplicationAddress.ToUInt64())
				{
					end = (long)maximumApplicationAddress.ToUInt64();
				}
				Debug.WriteLine("[DEBUG] memory scan starting... (start:0x" + start.ToString(MSize()) + " end:0x" + end.ToString(MSize()) + " time:" + DateTime.Now.ToString("h:mm:ss tt") + ")");
				UIntPtr uIntPtr = new UIntPtr((ulong)start);
				MEMORY_BASIC_INFORMATION lpBuffer = default(MEMORY_BASIC_INFORMATION);
				while (VirtualQueryEx(pHandle, uIntPtr, out lpBuffer).ToUInt64() != 0L && uIntPtr.ToUInt64() < (ulong)end && uIntPtr.ToUInt64() + (ulong)lpBuffer.RegionSize > uIntPtr.ToUInt64())
				{
					bool flag = lpBuffer.State == 4096;
					flag &= lpBuffer.BaseAddress.ToUInt64() < maximumApplicationAddress.ToUInt64();
					flag &= (lpBuffer.Protect & 0x100) == 0;
					flag &= (lpBuffer.Protect & 1) == 0;
					flag &= lpBuffer.Type == MEM_PRIVATE || lpBuffer.Type == MEM_IMAGE;
					unchecked
					{
						if (flag)
						{
							bool flag2 = (lpBuffer.Protect & 2) != 0;
							bool flag3 = (lpBuffer.Protect & 4u) != 0 || (lpBuffer.Protect & 8u) != 0 || (lpBuffer.Protect & 0x40u) != 0 || (lpBuffer.Protect & 0x80) != 0;
							bool flag4 = (lpBuffer.Protect & 0x10u) != 0 || (lpBuffer.Protect & 0x20u) != 0 || (lpBuffer.Protect & 0x40u) != 0 || (lpBuffer.Protect & 0x80) != 0;
							flag2 = flag2 && readable;
							flag3 = flag3 && writable;
							flag4 = flag4 && executable;
							flag = flag && (flag2 || flag3 || flag4);
						}
					}
					if (!flag)
					{
						uIntPtr = new UIntPtr(lpBuffer.BaseAddress.ToUInt64() + (ulong)lpBuffer.RegionSize);
					}
					else
					{
						MemoryRegionResult memoryRegionResult = default(MemoryRegionResult);
						memoryRegionResult.CurrentBaseAddress = uIntPtr;
						memoryRegionResult.RegionSize = lpBuffer.RegionSize;
						memoryRegionResult.RegionBase = lpBuffer.BaseAddress;
						MemoryRegionResult item2 = memoryRegionResult;
						uIntPtr = new UIntPtr(lpBuffer.BaseAddress.ToUInt64() + (ulong)lpBuffer.RegionSize);
						if (list.Count > 0)
						{
							MemoryRegionResult memoryRegionResult2 = list[list.Count - 1];
							if ((long)(ulong)memoryRegionResult2.RegionBase + memoryRegionResult2.RegionSize == (long)(ulong)lpBuffer.BaseAddress)
							{
								list[list.Count - 1] = new MemoryRegionResult
								{
									CurrentBaseAddress = memoryRegionResult2.CurrentBaseAddress,
									RegionBase = memoryRegionResult2.RegionBase,
									RegionSize = memoryRegionResult2.RegionSize + lpBuffer.RegionSize
								};
								continue;
							}
						}
						list.Add(item2);
					}
				}
				ConcurrentBag<long> bagResult = new ConcurrentBag<long>();
				Parallel.ForEach(list, delegate(MemoryRegionResult item, ParallelLoopState parallelLoopState, long index)
				{
					long[] array2 = CompareScan(item, aobPattern, mask);
					long[] array3 = array2;
					foreach (long item3 in array3)
					{
						bagResult.Add(item3);
					}
				});
				Debug.WriteLine("[DEBUG] memory scan completed. (time:" + DateTime.Now.ToString("h:mm:ss tt") + ")");
				return (from c in bagResult.ToList()
					orderby c
					select c).AsEnumerable();
			}));
		}

		/// <summary>
		/// Array of bytes scan
		/// </summary>
		/// <param name="code">Starting address or ini label</param>
		/// <param name="end">ending address</param>
		/// <param name="search">array of bytes to search for or your ini code label</param>
		/// <param name="file">ini file</param>
		/// <returns>First address found</returns>
		public async Task<long> AoBScan(string code, long end, string search, string file = "")
		{
			long start = checked((long)GetCode(code, file).ToUInt64());
			return (await AoBScan(start, end, search, true, true, true, file)).FirstOrDefault();
		}

		private unsafe long[] CompareScan(MemoryRegionResult item, byte[] aobPattern, byte[] mask)
		{
			if (mask.Length != aobPattern.Length)
			{
				throw new ArgumentException("aobPattern.Length != mask.Length");
			}
			checked
			{
				IntPtr intPtr = Marshal.AllocHGlobal((int)item.RegionSize);
				ReadProcessMemory(pHandle, item.CurrentBaseAddress, intPtr, (UIntPtr)(ulong)item.RegionSize, out var lpNumberOfBytesRead);
				int num = -aobPattern.Length;
				List<long> list = new List<long>();
				do
				{
					num = FindPattern(unchecked((byte*)intPtr.ToPointer()), (int)lpNumberOfBytesRead, aobPattern, mask, num + aobPattern.Length);
					if (num >= 0)
					{
						list.Add((long)(ulong)item.CurrentBaseAddress + num);
					}
				}
				while (num != -1);
				Marshal.FreeHGlobal(intPtr);
				return list.ToArray();
			}
		}

		private int FindPattern(byte[] body, byte[] pattern, byte[] masks, int start = 0)
		{
			int result = -1;
			checked
			{
				if (body.Length == 0 || pattern.Length == 0 || start > body.Length - pattern.Length || pattern.Length > body.Length)
				{
					return result;
				}
				for (int i = start; i <= body.Length - pattern.Length; i++)
				{
					if ((body[i] & masks[0]) != (pattern[0] & masks[0]))
					{
						continue;
					}
					bool flag = true;
					for (int j = 1; j <= pattern.Length - 1; j++)
					{
						if ((body[i + j] & masks[j]) != (pattern[j] & masks[j]))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						result = i;
						break;
					}
				}
				return result;
			}
		}

		private unsafe int FindPattern(byte* body, int bodyLength, byte[] pattern, byte[] masks, int start = 0)
		{
			int result = -1;
			checked
			{
				if (bodyLength <= 0 || pattern.Length == 0 || start > bodyLength - pattern.Length || pattern.Length > bodyLength)
				{
					return result;
				}
				for (int i = start; i <= bodyLength - pattern.Length; i++)
				{
					if ((body[i] & masks[0]) != (pattern[0] & masks[0]))
					{
						continue;
					}
					bool flag = true;
					for (int j = 1; j <= pattern.Length - 1; j++)
					{
						if ((body[i + j] & masks[j]) != (pattern[j] & masks[j]))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						result = i;
						break;
					}
				}
				return result;
			}
		}
	}
}
