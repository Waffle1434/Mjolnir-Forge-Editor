import io, zlib, time
from struct import *
from math import *

debug = False

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
        self.SeekBits(self.bitPos + count, io.SEEK_SET)

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
    def ReadStruct(self, format, count): return unpack('>'+format,self.ReadBytes(count))
    def ReadStructBits(self, format, count): return unpack('>'+format,self.ReadBits(count))
    def Seek(self, offset, mode=io.SEEK_CUR): self.SeekBits(8*offset,mode)
    def SeekBits(self, offset, mode=io.SEEK_CUR):
        if mode == io.SEEK_SET: self.bitPos = offset
        elif mode == io.SEEK_CUR: self.bitPos += offset
        else: raise
        self.byteStream.seek(floor(self.bitPos/8))


filename = "D:\Games\Steam\steamapps\common\Halo The Master Chief Collection\haloreach\map_variants\hr_forgeWorld_theCage.mvar"

f = open(filename, 'rb')
stream = BitStream(f)

#debug = True

def processPos(bitcount, rangesByAxis, out):
    # Determines the proper bitcounts to use for object position coordinates, given the map bounds and the baseline bitcount specified.
    MINIMUM_UNIT_16BIT = 0.00833333333 # hex 0x3C088889 == 0.00833333376795F
    if bitcount > 0x10: min_step = MINIMUM_UNIT_16BIT / (1 << (bitcount - 0x10))
    else: min_step = (1 << (0x10 - bitcount)) * MINIMUM_UNIT_16BIT
    
    if min_step >= 0.0001: # hex 0x38D1B717 == 9.99999974738e-05
        min_step *= 2
        for i in range(3):
            edx = min(0x800000, ceil(rangesByAxis[i] / min_step))
            ecx = (int(log(edx,2)) if edx >= 0 else 31) if edx else -1 # 23

            out[i] = min(26, 
                (ecx + (1 if ((edx & ((1 << ecx) - 1)) != 0) else 0)) if ecx != -1 
                else 0
            )
    else: out[0] = out[1] = out[2] = 26

def readShapeDimension(stream):
    eax = stream.ReadUInt16Bits(11)
    if not eax: return 0
    elif eax == 0x7FF: return 200
    else: return (eax - 1) * 0.0977517142892 + 0.0488758571446
def readShape(stream):
    shape = stream.ReadUInt8Bits(2)
    if shape < 1 or shape > 3: return

    shapeWidth = readShapeDimension(stream)

    if shape == 1: return
    elif shape == 3: shapeLength = readShapeDimension(stream)
    
    shapeTop = readShapeDimension(stream)
    shapeBottom = readShapeDimension(stream)


def readBlf(stream, size):
    stream.Seek(size - 8)
def readChdr(stream, size):
    stream.Seek(135)
    title = stream.ReadString16(128)
    description = stream.ReadString16(128)
    print('%s - %s' % (title,description))
    stream.Seek(49)
def readMvar(stream, size):
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

        #Position
        bitcount = 21
        axisBits = [0, 0, 0]
        a = stream.ReadBoolBit()
        if a:
            if mapBounds: processPos(bitcount, rangesByAxis, axisBits)
            else: print("POS TODO1")
        else:
            if mapBounds: processPos(bitcount, rangesByAxis, axisBits)
            elif not stream.ReadBoolBit() and stream.ReadUInt8Bits(2) != -1: print("POS TODO2")# != 3?!
        pos = [
            unpack('>I',b'\x00' + stream.ReadBits(axisBits[0]))[0],
            unpack('>I',b'\x00' + stream.ReadBits(axisBits[1]))[0],
            unpack('>I',b'\x00' + stream.ReadBits(axisBits[2]))[0]
        ]
        pos[0] = (0.5 + pos[0]) * (rangesByAxis[0] / (1 << axisBits[0])) + mapBounds[0][0]
        pos[1] = (0.5 + pos[1]) * (rangesByAxis[1] / (1 << axisBits[1])) + mapBounds[1][0]
        pos[2] = (0.5 + pos[2]) * (rangesByAxis[2] / (1 << axisBits[2])) + mapBounds[2][0]

        #print('%d - %s' % (i,pos))
        if i == 400:
            print('%d - %s' % (i,pos))
            print(pos[0] - 26.504993438720703 + pos[1] - 173.5377311706543 + pos[2] - 36.39936447143555)

        vertical = stream.ReadBoolBit()
        if vertical: axisAngleAxis = [0,0,1]
        else: stream.SeekBits(20)# TODO: load axisAngleAxis
        
        ang = stream.ReadUInt16Bits(14)
        #loadAxisAngleAngle(ang)
        spawnRelativeToMapIndex = stream.ReadUInt16Bits(10)

        #load data
        readShape(stream)
        
        eax = stream.ReadUInt8Bits(8)
        if eax & 0x80000000: eax |= 0xFFFFFF00 # test if signed
        spawnSequence = eax & 0xFF
        
        respawnTime = stream.ReadUInt8Bits(8)
        cachedType = stream.ReadUInt8Bits(5)
        forgeLabelIndex = -1 if stream.ReadBoolBit() else stream.ReadUInt8Bits(8)
        flags = stream.ReadUInt8Bits(8)
        team = stream.ReadUInt8Bits(4) - 1
        color = -1 if stream.ReadBoolBit() else stream.ReadUInt8Bits(3)
        
        if cachedType == 1:# weapon
            spareClips = stream.ReadUInt8Bits(8)
            continue
        elif cachedType <= 0xB: continue
        elif cachedType <= 0xE:# teleporter
            teleporterChannel = stream.ReadUInt8Bits(5)
            teleporterPassability = stream.ReadUInt8Bits(5)
        elif cachedType == 0x13: locationNameIndex = stream.ReadUInt8Bits(8) - 1
    tEl = (time.time_ns() - st) * 1e-6
    print('Loaded %d Objects (%dms)' % (i,tEl))


dictToFnc = { '_blf':readBlf, 'chdr':readChdr, 'mvar':readMvar }


try:
    while True:
        signature = stream.ReadString(4)
        
        fnc = dictToFnc.get(signature, None)
        if fnc != None:
            size = stream.ReadUInt32()
            print('%s (%d)' % (signature,size))
            fnc(stream, size)
        else: break
except Exception as ex:
    print(ex)
    pass

print("done")

