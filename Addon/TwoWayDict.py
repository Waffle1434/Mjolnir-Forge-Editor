def InverseDict(dict):
    invDict = {}
    for key, val in dict.items():
        if val not in invDict: invDict[val] = key
    return invDict

class TwoWayDict:    
    def __init__(self, A):
        self.AtoB = A
        self.BtoA = InverseDict(A)