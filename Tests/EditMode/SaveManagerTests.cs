using NUnit.Framework;
using UnityEngine;

namespace UniSave.Tests
{
    public class SaveManagerTests
    {
        private class TestData : ISavable
        {
            public string Key => "TestData";
            public int Priority => 10;
            public string Value = "Hello";

            public string CaptureStateJson() => JsonUtility.ToJson(this);
            public void RestoreStateJson(string json) => JsonUtility.FromJsonOverwrite(json, this);
        }

        private MockStorage _mockStorage;
        private TestData _testData;
        private SaveManager _saveManager;

        [SetUp]
        public void Setup()
        {
            // Create a fresh instance of SaveManager for each test
            var go = new GameObject("TestSaveManager");
            _saveManager = go.AddComponent<SaveManager>();

            _mockStorage = new MockStorage();
            _saveManager.Initialize(_mockStorage);

            _testData = new TestData();
            _saveManager.Register(_testData);
        }

        [TearDown]
        public void Teardown()
        {
            // Clean up the scene after each test
            Object.DestroyImmediate(_saveManager.gameObject);
        }

        [Test]
        public void SaveAsync_WritesDataToStorage()
        {
            // Act
            _saveManager.SaveAsync().GetAwaiter().GetResult();

            // Assert
            Assert.IsNotNull(_mockStorage.Data);
            StringAssert.Contains("TestData", _mockStorage.Data);
            StringAssert.Contains("Hello", _mockStorage.Data);
        }

        [Test]
        public void LoadAsync_RestoresData()
        {
            // Arrange
            _testData.Value = "Changed";
            _saveManager.SaveAsync().GetAwaiter().GetResult();

            _testData.Value = "ResetMe";

            // Act
            _saveManager.LoadAsync().GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("Changed", _testData.Value);
        }

        [Test]
        public void LoadAsync_VersionMismatch_DeletesData()
        {
            // Arrange
            // Injecting a fake save file with an incorrect version
            _mockStorage.Data = "{\"version\": 99, \"entries\": []}";

            // Act
            _saveManager.LoadAsync().GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(_mockStorage.DeleteCalled, "Save data should be deleted on version mismatch");
            Assert.IsNull(_mockStorage.Data);
        }
    }
}