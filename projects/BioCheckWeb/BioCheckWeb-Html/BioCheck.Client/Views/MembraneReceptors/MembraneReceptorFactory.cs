using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BioCheck.ViewModel.Cells;

namespace BioCheck.Views.MembraneReceptors
{

    public class MembraneReceptorInfo
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Angle { get; set; }
    }

    public static class MembraneReceptorFactory
    {
        public static List<MembraneReceptorInfo> Create(ContainerSizeTypes sizeType)
        {
            switch (sizeType)
            {
                case ContainerSizeTypes.One:
                    return new List<MembraneReceptorInfo>(new[]
                                       {
                                          //new MembraneReceptorInfo{Left=163, Top=-10, Angle=0},
                                          //new MembraneReceptorInfo{Left=275, Top=48, Angle=45},
                                          //new MembraneReceptorInfo{Left=315, Top=168, Angle=90},
                                          //new MembraneReceptorInfo{Left=265, Top=308, Angle=135},
                                          //new MembraneReceptorInfo{Left=163, Top=354, Angle=180},
                                          //new MembraneReceptorInfo{Left=33.5, Top=54, Angle=-45},
                                          //new MembraneReceptorInfo{Left=-8, Top=168, Angle=-90},
                                          //new MembraneReceptorInfo{Left=33, Top=308, Angle=292},

                                          new MembraneReceptorInfo{Left=163, Top=-10, Angle=0},
                                          new MembraneReceptorInfo{Left=275, Top=48, Angle=45},
                                          new MembraneReceptorInfo{Left=315, Top=168, Angle=90},
                                          new MembraneReceptorInfo{Left=265, Top=308, Angle=135},
                                          new MembraneReceptorInfo{Left=163, Top=354, Angle=180},
                                          new MembraneReceptorInfo{Left=33, Top=308, Angle=-135},
                                          new MembraneReceptorInfo{Left=-8, Top=168, Angle=-90},
                                          new MembraneReceptorInfo{Left=33.5, Top=54, Angle=-45},

                                       });
                    //break;
                case ContainerSizeTypes.Two:
                    return new List<MembraneReceptorInfo>(new[]
                                       {
                                            // 377,24
                                            //612,143
                                            //695,393
                                            //620,644
                                            //372,775
                                            //159,684
                                            //38.3332786560059,401.666656494141
                                            //156.666610717773,125.83332824707

                                          new MembraneReceptorInfo{Left=350, Top=0, Angle=0},
                                          new MembraneReceptorInfo{Left=587, Top=118, Angle=45},
                                          new MembraneReceptorInfo{Left=670, Top=368, Angle=90},
                                          new MembraneReceptorInfo{Left=595, Top=620, Angle=135},
                                          new MembraneReceptorInfo{Left=347, Top=750, Angle=180},
                                          new MembraneReceptorInfo{Left=131, Top=659, Angle=-135},
                                          new MembraneReceptorInfo{Left=13, Top=376, Angle=-90},
                                          new MembraneReceptorInfo{Left=134, Top=100, Angle=-45},
                                          
                                     
                                       });
                    //break;
                case ContainerSizeTypes.Three:
                    return new List<MembraneReceptorInfo>(new[]
                                       {
                                          new MembraneReceptorInfo{Left=551, Top=1, Angle=0},
                                          new MembraneReceptorInfo{Left=881, Top=166, Angle=45},
                                          new MembraneReceptorInfo{Left=1029, Top=554, Angle=90},
                                          new MembraneReceptorInfo{Left=924, Top=929, Angle=135},
                                          new MembraneReceptorInfo{Left=542, Top=1153, Angle=180},
                                          new MembraneReceptorInfo{Left=177, Top=953, Angle=-135},
                                          new MembraneReceptorInfo{Left=18, Top=578, Angle=-90},
                                          new MembraneReceptorInfo{Left=143, Top=175, Angle=-45},
                                       });
                    //break;
                default:
                    throw new ArgumentOutOfRangeException("sizeType");
            }
        }
    }
}
