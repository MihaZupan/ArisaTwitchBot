namespace ArisaTwitchBot
{
    public class RefLong
    {
        public long Value;

        public RefLong(long value)
        {
            Value = value;
        }

        public void Add(long value) => Value += value;

        public void Substract(long value) => Value -= value;
    }
}
