public interface ISerialization
{
    public byte[] Serialize(TetrisInventory inventory);

    public TetrisInventory Deserialize(byte[] bytes);
}
