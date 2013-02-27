namespace BioCheck.Views
{
    public interface IRelationshipTarget
    {
        object DataContext { set; get; }

        int FixedWidth { set; get; }
        int FixedHeight { set; get; }
    }
}