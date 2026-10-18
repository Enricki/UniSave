using Cysharp.Threading.Tasks;

namespace UniSave.Tests
{
    public class MockStorage : ISaveStorage
    {
        public string Data { get; set; }
        public bool DeleteCalled { get; private set; }

        public bool HasData() => !string.IsNullOrEmpty(Data);

        public UniTask WriteAsync(string data)
        {
            Data = data;
            return UniTask.CompletedTask;
        }

        public UniTask<string> ReadAsync()
        {
            return UniTask.FromResult(Data);
        }

        public UniTask DeleteAsync()
        {
            Data = null;
            DeleteCalled = true;
            return UniTask.CompletedTask;
        }
    }
}