namespace Model.ModelSql
{
    public class ActivityType : DescriptiveType, ICloneable
    {
        public bool IsDefault { get; set; }

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
