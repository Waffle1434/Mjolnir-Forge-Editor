import io, zlib, time
from struct import *
from math import *

#debug = False

class BitStream:
    def __init__(self, byteStream):
        self.byteStream = byteStream
        self.bitPos = 0
    
    def ReadBytes(self, count): return self.ReadBits(8*count)
    def ReadBits(self, count):
        finalPos = self.bitPos + count
        lShift = finalPos % 8
        rShift = -finalPos % 8
        skip = self.bitPos % 8 + rShift >= 8
        carryMask = 2**rShift - 1# redundant because of shift?
        fullBitMask = 2**count - 1 << rShift

        byteCount = ceil(finalPos/8) - floor(self.bitPos/8)
        bytes = self.byteStream.read(byteCount)
        self.SeekBits(count)

        #if debug: print('Read %d bits' % count)

        outBytes = bytearray()
        carry = 0
        #if debug: print('masked: %s & %s = %s' % (format(by,'08b'),format(firstMask,'08b'),format(by & firstMask,'08b')))
        for i in range(byteCount):
            by = bytes[i] & (fullBitMask >> 8*(byteCount-1 - i) & 0xFF)
            carryNext = (by & carryMask) << lShift# TODO: overflow?!
            #if carryNext > 255: raise

            '''if debug:
                curBitPos = self.bitPos-count + i*8
                print('%d+%d\t%s c: & %s << %d = %s' % (int(curBitPos/8),curBitPos % 8, format(by,'08b'), format(carryMask,'08b'),lShift,format(carryNext,'08b')))'''

            if not skip: 
                outBytes.append((by >> rShift) | carry)
                #if debug: print('out: %s >> %d = %s | %s = %s' % (format(by,'08b'),rShift,format(by >> rShift,'08b'),format(carry,'08b'),format(outBytes[-1],'08b')))
            else: skip = False
            #elif debug: print("skipped")
            #if debug: print("")
            carry = carryNext
        return outBytes
        
    def ReadString(self, count, stopOnNull=False): return self.ReadBytes(count).decode('utf-8',errors='ignore').rstrip('\0')
    def ReadString16(self, count, stopOnNull=False):
        if stopOnNull:
            s = ""
            for i in range(2*count):
                char = self.ReadBytes(2)
                if char == b'\x00\x00': break
                s += char.decode('utf-16-be',errors='ignore')
            return s
        else: return self.ReadBytes(2*count).decode('utf-16-be',errors='ignore').rstrip('\0')
    def ReadUInt32(self): return unpack('>I',self.ReadBytes(4))[0]
    def ReadUInt16(self): return unpack('>H',self.ReadBytes(2))[0]
    def ReadFloat(self): return unpack('>f',self.ReadBytes(4))[0]
    def ReadBoolBit(self): return unpack('?',self.ReadBits(1))[0]
    def ReadUInt8Bits(self, count): return unpack('B',self.ReadBits(count))[0]
    def ReadUInt16Bits(self, count): return unpack('>H',self.ReadBits(count))[0]
    def ReadUInt32Bits(self, count): return unpack('>I',self.ReadBits(count))[0]
    def ReadUInt64Bits(self, count): return unpack('>L',self.ReadBits(count))[0]
    def ReadUInt128Bits(self, count): return unpack('>Q',self.ReadBits(count))[0]
    def ReadUIntBits(self, count): return int.from_bytes(self.ReadBits(count),'big')
    def ReadStruct(self, format, count): return unpack('>'+format,self.ReadBytes(count))
    def ReadStructBits(self, format, count): return unpack('>'+format,self.ReadBits(count))
    def Seek(self, offset, mode=io.SEEK_CUR): self.SeekBits(8*offset,mode)
    def SeekBits(self, offset, mode=io.SEEK_CUR):
        if mode == io.SEEK_SET: self.bitPos = offset
        elif mode == io.SEEK_CUR: self.bitPos += offset
        else: raise
        self.byteStream.seek(floor(self.bitPos/8))

def ReadShapeDimension(stream):
    value = stream.ReadUInt16Bits(11)
    if not value: return 0
    elif value == 0x7FF: return 200
    else: return (value - 1) * 0.0977517142892 + 0.0488758571446

def ReadBlamEngineFileHeader(stream, size):
    stream.Seek(size - 8)
def ReadContentHeader(stream, size):
    stream.Seek(135)
    title = stream.ReadString16(128)
    description = stream.ReadString16(128)
    print('%s - %s' % (title,description))
    stream.Seek(49)
def ReadMapVariant(stream, size):
    startPos = stream.bitPos
    stream.SeekBits(110*8 + 7)
    title = stream.ReadString16(128,True)
    description = stream.ReadString16(128,True)
    print('%s - %s' % (title,description))

    stream.SeekBits(startPos + 223*8 + 2, io.SEEK_SET)
    mapBounds = (stream.ReadStruct('ff',8), stream.ReadStruct('ff',8), stream.ReadStruct('ff',8))
    #from collections import namedtuple
    print('Bounds: %s' % str(mapBounds))

    rangesByAxis = [
        mapBounds[0][1] - mapBounds[0][0],
        mapBounds[1][1] - mapBounds[1][0],
        mapBounds[2][1] - mapBounds[2][0]
    ]

    if mapBounds:# Determines the proper bitcounts to use for object position coordinates, given the map bounds and the baseline bitcount specified.
        bitcount = 21
        MINIMUM_UNIT_16BIT = 0.00833333333 # hex 0x3C088889 == 0.00833333376795F
        if bitcount > 16: min_step = MINIMUM_UNIT_16BIT / (1 << (bitcount - 16))
        else: min_step = (1 << (16 - bitcount)) * MINIMUM_UNIT_16BIT
        
        axisBits = [0,0,0]
        if min_step >= 0.0001: # hex 0x38D1B717 == 9.99999974738e-05
            min_step *= 2
            for i in range(3):
                edx = min(0x800000, ceil(rangesByAxis[i] / min_step))
                ecx = (int(log(edx,2)) if edx >= 0 else 31) if edx else -1 # 23

                axisBits[i] = min(26, 
                    (ecx + (1 if ((edx & ((1 << ecx) - 1)) != 0) else 0)) if ecx != -1 
                    else 0
                )
        else: axisBits = [26,26,26]
    
    stream.SeekBits(startPos + 255*8 + 2, io.SEEK_SET)
    strCount = stream.ReadUInt16Bits(9)
    print('%d Gametype Labels' % strCount)

    #global debug
    #debug = True

    if strCount > 0:
        offsets = []
        for i in range(strCount):
            presence = stream.ReadBoolBit()
            if not presence: continue
            offset = stream.ReadUInt16Bits(12)
            offsets.append(offset)

        dataLength   = stream.ReadUInt16Bits(13)
        isCompressed = stream.ReadBoolBit()
        if isCompressed:
            compSize   = stream.ReadUInt16Bits(13)
            stream.Seek(4) # skip zlib header's uncompressed size
            decodedStrs = zlib.decompress(stream.ReadBytes(compSize-4))
            if len(decodedStrs) != dataLength:
                print('Expected ~%d bytes got %d (compressed into %d)' % (dataLength,len(decodedStrs),compSize))
            decodedStrs = decodedStrs.decode('utf-8').rstrip('\0')
        else:
            decodedStrs = stream.ReadString(dataLength)
        
        labelStrs = decodedStrs.upper().split('\0')
        print(labelStrs)
    
    st = time.time_ns()
    for i in range(651):
        presence = stream.ReadBoolBit()
        if not presence: break
        unk00 = stream.ReadUInt8Bits(2)
        noSubcat = not stream.ReadBoolBit()
        if noSubcat: subcat = stream.ReadUInt8Bits(8)
        absence = stream.ReadBoolBit()
        if absence: objType = 0xFF
        else: objType = stream.ReadUInt8Bits(5)

        # Position
        a = stream.ReadBoolBit()
        if not mapBounds:
            print("POS TODO")
            if not a and not stream.ReadBoolBit() and stream.ReadUInt8Bits(2) != -1: print("POS TODO2")# != 3?!
            raise
            
        pos = [
            (0.5 + unpack('>I',b'\x00' + stream.ReadBits(axisBits[0]))[0]) * (rangesByAxis[0] / (1 << axisBits[0])) + mapBounds[0][0],
            (0.5 + unpack('>I',b'\x00' + stream.ReadBits(axisBits[1]))[0]) * (rangesByAxis[1] / (1 << axisBits[1])) + mapBounds[1][0],
            (0.5 + unpack('>I',b'\x00' + stream.ReadBits(axisBits[2]))[0]) * (rangesByAxis[2] / (1 << axisBits[2])) + mapBounds[2][0]
        ]

        #print('%d - %s' % (i,pos))
        if i == 400:# Debugging
            print('%d - %s' % (i,pos))
            print(pos[0] - 26.504993438720703 + pos[1] - 173.5377311706543 + pos[2] - 36.39936447143555)

        vertical = stream.ReadBoolBit()
        if vertical: axisAngleAxis = [0,0,1]
        else: stream.SeekBits(20)# TODO: load axisAngleAxis
        
        ang = stream.ReadUInt16Bits(14)
        #loadAxisAngleAngle(ang)
        spawnRelativeToMapIndex = stream.ReadUInt16Bits(10)

        # Load Object Data
        shape = stream.ReadUInt8Bits(2)
        if shape > 0 and shape < 4:
            shapeWidth = ReadShapeDimension(stream)
            if shape == 3: shapeLength = ReadShapeDimension(stream)
            if shape != 1:
                shapeTop = ReadShapeDimension(stream)
                shapeBottom = ReadShapeDimension(stream)
        
        eax = stream.ReadUInt8Bits(8)
        if eax & 0x80000000: eax |= 0xFFFFFF00 # test if signed
        spawnSequence = eax & 0xFF
        
        respawnTime = stream.ReadUInt8Bits(8)
        cachedType = stream.ReadUInt8Bits(5)
        forgeLabelIndex = -1 if stream.ReadBoolBit() else stream.ReadUInt8Bits(8)
        flags = stream.ReadUInt8Bits(8)
        team = stream.ReadUInt8Bits(4) - 1
        color = -1 if stream.ReadBoolBit() else stream.ReadUInt8Bits(3)
        
        if cachedType == 1: spareClips = stream.ReadUInt8Bits(8)# weapon
        elif cachedType > 11 and cachedType <= 14:# teleporter
            teleporterChannel = stream.ReadUInt8Bits(5)
            teleporterPassability = stream.ReadUInt8Bits(5)
        elif cachedType == 19: locationNameIndex = stream.ReadUInt8Bits(8) - 1
    tEl = (time.time_ns() - st) * 1e-6
    print('Loaded %d Objects (%dms)' % (i,tEl))
def TryReadMvarFile(filename):
    f = open(filename, 'rb')
    stream = BitStream(f)

    try:
        while True:
            signature = stream.ReadString(4)
            
            fnc = dictToFnc.get(signature, None)
            if fnc != None:
                size = stream.ReadUInt32()
                print('%s (%d)' % (signature,size))
                fnc(stream, size)
            else: break
        return True
    except Exception as ex:
        print(ex)
        pass
    return False

dictToFnc = { '_blf':ReadBlamEngineFileHeader, 'chdr':ReadContentHeader, 'mvar':ReadMapVariant }

TryReadMvarFile("D:\Games\Steam\steamapps\common\Halo The Master Chief Collection\haloreach\map_variants\hr_forgeWorld_theCage.mvar")
print("done")