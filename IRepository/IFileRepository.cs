namespace ImagesToPdfApi.IRepository
{
    // FileRepository.cs
    public interface IFileRepository
    {
        Task<byte[]> ConvertImagesToPdfAsync(IFormFileCollection files);
    }

}
