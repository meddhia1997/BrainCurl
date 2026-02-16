using UnityEngine.SceneManagement;

public sealed class RestartService
{
    private readonly SaveLoadService _saveLoad;

    public RestartService(SaveLoadService saveLoad)
    {
        _saveLoad = saveLoad;
    }

    public void Restart(bool clearSave)
    {
        if (clearSave)
            _saveLoad.Delete();

        // simplest + safest: reload scene (guarantees all systems reset cleanly)
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
