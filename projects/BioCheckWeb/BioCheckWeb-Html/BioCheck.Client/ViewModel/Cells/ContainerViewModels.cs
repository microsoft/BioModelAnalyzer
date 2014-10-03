using System.Linq;
using MvvmFx.Common.ViewModels;

namespace BioCheck.ViewModel.Cells
{
    public class ContainerViewModels : ViewModelCollection<ContainerViewModel>
    {
        /// <summary>
        /// Determines whether the specified position X and Y has a container.
        /// </summary>
        /// <param name="positionX">The position X.</param>
        /// <param name="positionY">The position Y.</param>
        /// <returns>
        ///   <c>true</c> if the specified position X has container; otherwise, <c>false</c>.
        /// </returns>
        public bool HasContainer(int positionX, int positionY)
        {
            bool hasContainer = false;

            // Check if there's a container in this exact position
            hasContainer = (from cvm in this
                            where cvm.PositionX == positionX
                                  && cvm.PositionY == positionY
                            select cvm).FirstOrDefault()
                               != null;

            return hasContainer;
        }

        /// <summary>
        /// Determines whether the specified position X and Y has a container or is overlapped by a large sized container in a different 
        /// </summary>
        /// <param name="positionX">The position X.</param>
        /// <param name="positionY">The position Y.</param>
        /// <returns>
        ///   <c>true</c> if the specified position X has container; otherwise, <c>false</c>.
        /// </returns>
        public bool HasContainerOverlapping(int positionX, int positionY)
        {
            bool hasContainer = false;

            // Check if there's a Size One in this position
            hasContainer = (from cvm in this
                               where cvm.PositionX == positionX
                                     && cvm.PositionY == positionY
                               select cvm).FirstOrDefault()
                               != null;

            if(!hasContainer)
            {
                // Check if there's a Size Two that involves this position.
                // This means checking the three positions around this one as the bottom-right corner
                hasContainer = (from cvm in this
                                where 
                                ((cvm.PositionX == positionX - 1 && cvm.PositionY == positionY)
                                ||
                                (cvm.PositionX == positionX - 1 && cvm.PositionY == positionY - 1)
                                ||
                                (cvm.PositionX == positionX && cvm.PositionY == positionY - 1)
                                )
                                && (cvm.SizeTwo || cvm.SizeThree)
                                select cvm).FirstOrDefault()
                            != null;


                if (!hasContainer)
                {
                    // Check if there's a Size Three that involves this position
                    // This means checking the 8 positions around this one as the bottom-right corner
                    hasContainer = (from cvm in this
                                    where
                                    ((cvm.PositionX == positionX - 2 && cvm.PositionY == positionY)
                                    ||
                                    (cvm.PositionX == positionX - 2 && cvm.PositionY == positionY - 1)
                                    ||
                                    (cvm.PositionX == positionX - 2 && cvm.PositionY == positionY - 2)
                                      ||
                                    (cvm.PositionX == positionX - 1 && cvm.PositionY == positionY - 2)
                                    ||
                                    (cvm.PositionX == positionX && cvm.PositionY == positionY - 2)
                                    )
                                    && cvm.SizeThree
                                    select cvm).FirstOrDefault()
                                != null;
                }
            }

            return hasContainer;
        }
    }
}