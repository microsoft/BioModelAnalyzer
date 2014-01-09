

type MA = A1 | A2 | A3
type MB = B1 | B2
type MC = C1 of int | C2 of string

let table = dict [  (A1,B1,C1 1),"1.1.1-1";
                    (A1,B2,C1 2),"1.1.1-2";
                 ]

table.[A1,B1,C1 1]
