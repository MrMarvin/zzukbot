using SharpDX;
using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using ZzukBot.ExtensionFramework.Interfaces;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

[Export(typeof(IPlugin))]
public class ExampleRender : IPlugin
{
    public void Hook_EndScene(IntPtr Device)
    {
        if(!Renderer.Instance.Initialized)
        {
            if(!Renderer.Instance.Initialize(Device))
            {
                // NOTE: Initialization failed.
                return;
            }
        }
        
        // NOTE: Buffer up render calls
        Renderer.Instance.PushRectFilled(5, 5, 100, 100, Renderer.Color(0, 255, 0));
        Renderer.Instance.PushRectOutline(4, 4, 102, 102, Renderer.Color(0, 0, 255));

        if(ObjectManager.Instance.IsIngame)
        {
            foreach(WoWUnit Player in ObjectManager.Instance.Players)
            {
                Vector2 Screen = DirectX.Instance.World2Screen(Player.Position);
                Renderer.Instance.PushLine(Renderer.Instance.ScreenWidth / 2.0f, Renderer.Instance.ScreenHeight / 2.0f, Screen.X, Screen.Y, Renderer.Color(0, 0, 255));
            }
        }

        // NOTE: Draws buffered render calls, call once per frame after you are done.
        Renderer.Instance.Render();
    }

    public bool Load()
    {
        DirectX.Instance.OnEndSceneExecution += Hook_EndScene;
        return true;
    }

    public void Unload()
    {
        DirectX.Instance.OnEndSceneExecution -= Hook_EndScene;
    }

    public void ShowGui()
    {
        MessageBox.Show("This plugin does not have a gui.");
    }

    public void Dispose()
    {
        if(Renderer.Instance.Initialized)
        {
            Renderer.Instance.Dispose();
        }
    }

    public string Author { get { return "Blacknight"; } }
    public string Name { get { return "ExampleRender"; } }
    public int Version { get { return 1; } }
}
