module Tube

open Cells

let distalTop = Array.append[|Cell(1.,15.,Markers = DTC); Cell(2.4,15.,Markers = Stopped);Cell(2.7,14.3,Markers = Stopped);Cell(3.,13.6,Markers = Stopped);Cell(3.3,12.9,Markers = Stopped);Cell(3.6,12.2,Markers = Stopped);Cell(3.9,12.2,Markers = Stopped);Cell(4.2,12.2,Markers = Stopped);
                         Cell(4.5,11.5,Markers = Stopped);Cell(4.8,11.5,Markers = Stopped);Cell(5.1,11.5,Markers = Stopped);Cell(5.4,11.5,Markers = Stopped);Cell(5.7,11.5,Markers = Stopped);Cell(6.,11.2,Markers = Stopped);
                         Cell(6.3,10.5,Markers = Stopped);Cell(6.6,10.5,Markers = Stopped);Cell(6.9,10.5,Markers = Stopped);Cell(7.2,10.5,Markers = Stopped);Cell(7.5,10.5,Markers = Stopped);Cell(7.8,9.8,Markers = Stopped);
                         Cell(8.1,9.1,Markers = Stopped);Cell(8.4,9.1,Markers = Stopped);Cell(8.7,9.1,Markers = Stopped);Cell(9.,9.1,Markers = Stopped);Cell(9.3,9.1,Markers = Stopped);Cell(9.6,8.4,Markers = Stopped);Cell(9.9,7.7,Markers = Stopped)|] 
                       (Array.init 15 (fun x -> Cell(border - 10. + float x*0.7, topTubeT,Markers = Stopped)))
let distalBottom = Array.append [|Cell(2.7,15.7,Markers = Stopped);Cell(3.,16.4,Markers = Stopped);Cell(3.3,17.1,Markers = Stopped);Cell(3.6,17.8,Markers = Stopped);Cell(3.9,17.8,Markers = Stopped);Cell(4.2,17.8,Markers = Stopped);
                            Cell(4.5,18.5,Markers = Stopped);Cell(4.8,18.5,Markers = Stopped);Cell(5.1,18.5,Markers = Stopped);Cell(5.4,18.5,Markers = Stopped);Cell(5.7,18.5,Markers = Stopped);Cell(6.,18.8,Markers = Stopped);
                            Cell(6.3,19.5,Markers = Stopped);Cell(6.6,19.5,Markers = Stopped);Cell(6.9,19.5,Markers = Stopped);Cell(7.2,19.5,Markers = Stopped);Cell(7.5,19.5,Markers = Stopped);Cell(7.8,20.2,Markers = Stopped);
                            Cell(8.1,20.9,Markers = Stopped);Cell(8.4,20.9,Markers = Stopped);Cell(8.7,20.9,Markers = Stopped);Cell(9.,20.9,Markers = Stopped);Cell(9.3,20.9,Markers = Stopped);Cell(9.6,21.6,Markers = Stopped);Cell(9.9,22.3,Markers = Stopped)|]
                          (Array.init 15 (fun x -> Cell(border - 10. + float x*0.7, topTubeB,Markers = Stopped)))//

let outerBend = Array.append (Array.append (Array.append(Array.append(Array.append (Array.init 8 (fun x -> Cell(117. + float x * 1.,topTubeT + float (x)*0.35, Markers = Stopped))) 
                                                                     (Array.init 8 (fun x -> Cell(117.7 + float x * 1.,topTubeT + 0.2 + float (x)*0.35, Markers = Stopped))))
                                                       (Array.append (Array.init 5 (fun x -> Cell(125. + float x * 1.,topTubeT+2.8 + float (x)*0.7, Markers = Stopped))) 
                                                                     (Array.init 5 (fun x -> Cell(125.7 + float x * 1.,topTubeT+3.15 + float (x)*0.7, Markers = Stopped)))))
                                                        (Array.append (Array.init 5 (fun x -> Cell(129. + float x * 1.,topTubeT+6. + float (x)*1.4, Markers = Stopped))) 
                                                                     (Array.init 4 (fun x -> Cell(129.7 + float x * 1.,topTubeT+6.35 + float (x)*1.4, Markers = Stopped)))))
                                           (Array.init 13 (fun x -> Cell(133.,19. + float x *0.7,Markers = Stopped))))
                             (Array.append(Array.append(Array.append (Array.init 8 (fun x -> Cell(125.4 - float x * 1.,bottomTubeB-2.8+ float (x)*0.35, Markers = Stopped))) 
                                                       (Array.init 8 (fun x -> Cell(124.7 - float x * 1.,bottomTubeB-2.6 + float (x)*0.35, Markers = Stopped))))
                                         (Array.append (Array.init 4 (fun x -> Cell(125.4 + float x * 1.,bottomTubeB-2.8 - float (x)*0.7, Markers = Stopped))) 
                                                       (Array.init 5 (fun x -> Cell(124.7 + float x * 1.,bottomTubeB-2.55 - float (x)*0.7, Markers = Stopped)))))
                                         (Array.append (Array.init 4 (fun x -> Cell(129.4 + float x * 1.,bottomTubeB-6. - float (x)*1.4, Markers = Stopped))) 
                                                       (Array.init 5 (fun x -> Cell(128.7 + float x * 1.,bottomTubeB-5.65 - float (x)*1.4, Markers = Stopped)))))
let innerBend = Array.append (Array.append (Array.init 2 (fun x -> Cell(117. + float x * 1.,topTubeB + float (x)*1.4, Markers = Stopped))) 
                                           (Array.init 1 (fun x -> Cell(117.7 + float x * 1.,topTubeB + baseCell + float (x)*1.4, Markers = Stopped))))
                               (Array.append (Array.init 2 (fun x -> Cell(117. + float x * 1.,bottomTubeT - float (x)*1.4, Markers = Stopped))) 
                                             (Array.init 1 (fun x -> Cell(117.7 + float x * 1.,bottomTubeT-baseCell - float (x)*1.4, Markers = Stopped))))

let rachis = Array.append (Array.append (Array.append (Array.append (Array.append (Array.init 10 (fun x -> Cell(border-10.+float x*0.7,topTubeB- float x * 0.5, Markers=Stopped)))
                                                                                  (Array.init 10 (fun x -> Cell(border-3.7+float x* 0.7, topTubeB-4.5,Markers=Stopped))))
                                                                    (Array.init 7 (fun x -> Cell(border+3.3+float x* 0.5, topTubeB-4.5 + float x* 0.7,Markers=Stopped))))
                                                      (Array.init 6 (fun y -> Cell(border+6.3,topTubeB-0.3 + float y * 0.7,Markers=Stopped))))
                                        (Array.init 7 (fun x -> Cell(border+6.3- float x * 0.7, topTubeB+3.3+float x * 0.5,Markers=Stopped))))
                          (Array.init 30 (fun x -> Cell(border+2.1-float x * 0.7, topTubeB+6.3- float x *0.1,Markers=Stopped)))
let proximal = Array.append (Array.init 75 (fun y -> Cell(border - float y * 0.7,bottomTubeT, Markers = Stopped)))
                         (Array.init 75 (fun y -> Cell(border - float y * 0.7,bottomTubeB, Markers = Stopped)))
                         
let cellsE = Array.append rachis (Array.append innerBend (Array.append outerBend (Array.append proximal (Array.append distalTop distalBottom))))//

//S for smaller tube...
let outerBendS = Array.append (Array.append (Array.append(Array.append (Array.init 8 (fun x -> Cell(41. + float x * 1.,7.7 + float (x)*0.35, Markers = Stopped))) 
                                                                     (Array.init 8 (fun x -> Cell(41.7 + float x * 1.,7.9 + float (x)*0.35, Markers = Stopped))))
                                                       (Array.append (Array.init 15 (fun x -> Cell(49. + float x * 1.,10.5 + float (x)*1.4, Markers = Stopped))) 
                                                                     (Array.init 15 (fun x -> Cell(49.7 + float x * 1.,10.85 + float (x)*1.4, Markers = Stopped)))))
                                           (Array.init 11 (fun x -> Cell(63.4,30.5 + float x *0.7,Markers = Stopped))))
                            (Array.append(Array.append (Array.init 8 (fun x -> Cell(49.4 - float x * 1.,57.1 + float (x)*0.35, Markers = Stopped))) 
                                                       (Array.init 8 (fun x -> Cell(48.7 - float x * 1.,57.3 + float (x)*0.35, Markers = Stopped))))
                                         (Array.append (Array.init 15 (fun x -> Cell(63.4- float x * 1.,37.5 + float (x)*1.4, Markers = Stopped))) 
                                                       (Array.init 15 (fun x -> Cell(62.7 - float x * 1.,37.85 + float (x)*1.4, Markers = Stopped)))))

let innerBendS = Array.append (Array.append (Array.append (Array.init 8 (fun x -> Cell(41. + float x * 1.,22.3 + float (x)*1.4, Markers = Stopped))) 
                                                           (Array.init 8 (fun x -> Cell(41.7 + float x * 1.,23. + float (x)*1.4, Markers = Stopped))))
                                              [|Cell(49.4,33.5,Markers = Stopped);Cell(49.4,34.2,Markers = Stopped);Cell(49.4,34.9,Markers = Stopped);Cell(49.4,35.6,Markers = Stopped)|])
                               (Array.append (Array.init 8 (fun x -> Cell(49.4 - float x * 1.,35.6 + float (x)*1.4, Markers = Stopped))) 
                                             (Array.init 8 (fun x -> Cell(48.7 - float x * 1.,36.3 + float (x)*1.4, Markers = Stopped))))

let proximalS = Array.append (Array.init 25 (fun y -> Cell(41. - float y * 0.7,46.1, Markers = Stopped)))
                         (Array.init 25 (fun y -> Cell(41. - float y * 0.7,60.1, Markers = Stopped)))

let cellsES = Array.append innerBendS (Array.append outerBendS (Array.append proximalS (Array.append distalTop distalBottom)))