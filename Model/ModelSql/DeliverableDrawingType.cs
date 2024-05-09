namespace Model.ModelSql
{
    public class DeliverableDrawingType : DescriptiveType, ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
