using D3D9 = SharpDX.Direct3D9;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class Renderer
{
    public static Renderer Instance = new Renderer();
    public bool Initialized;
    public List<VertexData> Vertices;
    public List<RenderCommand> Commands;

    public D3D9.StateBlock StateBlock;
    public D3D9.VertexDeclaration Declaration;
    public D3D9.VertexBuffer VertexBuffer;
    public D3D9.Device Device;
    
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public uint Color;

        public float U;
        public float V;
    }
    
    public interface RenderCommand {}

    public class RenderCommand_DrawPrimitive : RenderCommand
    {
        public D3D9.PrimitiveType PrimitiveType;
        public int PrimitiveCount;
        public int VertexCount;
    }

    public class RenderCommand_SetTexture : RenderCommand
    {
        public D3D9.BaseTexture Texture;
    }
    
    public static uint Color(byte R, byte G, byte B)
    {
        return Color(0xFF, R, G, B);
    }

    public static uint Color(byte A, byte R, byte G, byte B)
    {
        return ((uint)(((A)<<24)|((R)<<16)|((G)<<8)|(B)));
    }
    
    public bool Initialize(IntPtr Device)
    {
        return Initialize((D3D9.Device)Device);
    }

    public bool Initialize(D3D9.Device Device)
    {
        Dispose();

        D3D9.VertexElement[] Elements = new D3D9.VertexElement[]
        {
            new D3D9.VertexElement(0, 0, D3D9.DeclarationType.Float4, 0, D3D9.DeclarationUsage.PositionTransformed, 0),
            new D3D9.VertexElement(0, 16, D3D9.DeclarationType.Color, 0, D3D9.DeclarationUsage.Color, 0),
            new D3D9.VertexElement(0, 20, D3D9.DeclarationType.Float2, 0, D3D9.DeclarationUsage.TextureCoordinate, 0),
            D3D9.VertexElement.VertexDeclarationEnd
        };

        D3D9.VertexDeclaration Declaration = new D3D9.VertexDeclaration(Device, Elements);

        if(Declaration != null)
        {
            // TODO: Crash waiting to happen by using default pool, need reset hook.
            // "D3DUSAGE_DYNAMIC and D3DPOOL_MANAGED are incompatible and should not be used together."
            D3D9.VertexBuffer VertexBuffer = new D3D9.VertexBuffer(Device, Marshal.SizeOf(typeof(VertexData)) * 2048, D3D9.Usage.Dynamic | D3D9.Usage.WriteOnly, 0, D3D9.Pool.Default);

            if(VertexBuffer != null)
            {
                D3D9.StateBlock StateBlock = new D3D9.StateBlock(Device, D3D9.StateBlockType.All);

                if(StateBlock != null)
                {
                    this.StateBlock = StateBlock;
                    this.Declaration = Declaration;
                    this.VertexBuffer = VertexBuffer;

                    Vertices = new List<VertexData>(2048);
                    Commands = new List<RenderCommand>(1024);
                    this.Device = Device;
                    Initialized = true;
                 }
            }
        }

        return Initialized;
    }

    public void PushVertex(float X, float Y, uint Color)
    {
        VertexData Vertex = new VertexData();
        Vertex.X = X;
        Vertex.Y = Y;
        Vertex.Z = 0;
        Vertex.W = 1;
        Vertex.Color = Color;
        Vertex.U = 0;
        Vertex.V = 0;
        Vertices.Add(Vertex);
    }

    public void PushVertexUV(float X, float Y, float U, float V, uint Color)
    {
        VertexData Vertex = new VertexData();
        Vertex.X = X;
        Vertex.Y = Y;
        Vertex.Z = 0;
        Vertex.W = 1;
        Vertex.Color = Color;
        Vertex.U = U;
        Vertex.V = V;
        Vertices.Add(Vertex);
    }

    public void PushDrawCommand(D3D9.PrimitiveType PrimitiveType, int PrimitiveCount, int VertexCount)
    {
        RenderCommand_DrawPrimitive Command = new RenderCommand_DrawPrimitive();
        Command.PrimitiveType = PrimitiveType;
        Command.PrimitiveCount = PrimitiveCount;
        Command.VertexCount = VertexCount;
        Commands.Add(Command);
    }
    
    public void PushRectFilled(float X, float Y, float Width, float Height, uint Color)
    {
        PushVertex(X, Y + Height, Color);
        PushVertex(X, Y, Color);
        PushVertex(X + Width, Y + Height, Color);
        PushVertex(X + Width, Y, Color);
        PushDrawCommand(D3D9.PrimitiveType.TriangleStrip, 2, 4);
    }

    public void PushRectOutline(float X, float Y, float Width, float Height, uint Color)
    {
        --Width;
        --Height;
    
        PushVertex(X, Y, Color);
        PushVertex(X + Width, Y, Color);
        PushVertex(X + Width, Y + Height, Color);
        PushVertex(X, Y + Height, Color);
        PushVertex(X, Y, Color);
        PushDrawCommand(D3D9.PrimitiveType.LineStrip, 4, 5);
    }

    public void PushLine(float X, float Y, float X2, float Y2, uint Color)
    {
        PushVertex(X, Y, Color);
        PushVertex(X2, Y2, Color);
        PushDrawCommand(D3D9.PrimitiveType.LineStrip, 1, 2);
    }

    public void Render()
    {
        StateBlock.Capture();

        Device.VertexDeclaration = Declaration;
        Device.VertexShader = null;
        Device.PixelShader = null;
        Device.SetTexture(0, null);
        Device.SetRenderState(D3D9.RenderState.ScissorTestEnable, false);
        Device.SetRenderState(D3D9.RenderState.CullMode, D3D9.Cull.None);
        Device.SetRenderState(D3D9.RenderState.Lighting, false);
        Device.SetRenderState(D3D9.RenderState.ZEnable, false);
        Device.SetRenderState(D3D9.RenderState.AlphaBlendEnable, true);
        Device.SetRenderState(D3D9.RenderState.BlendOperation, D3D9.BlendOperation.Add);
        Device.SetRenderState(D3D9.RenderState.AlphaTestEnable, false);
        Device.SetRenderState(D3D9.RenderState.SourceBlend, D3D9.Blend.SourceAlpha);
        Device.SetRenderState(D3D9.RenderState.DestinationBlend, D3D9.Blend.InverseSourceAlpha);

        SharpDX.DataStream Buffer = VertexBuffer.Lock(0, 0, D3D9.LockFlags.Discard);

        if(Buffer != null)
        {
            Buffer.WriteRange<VertexData>(Vertices.ToArray());
            VertexBuffer.Unlock();
        }

        Device.SetStreamSource(0, VertexBuffer, 0, Marshal.SizeOf(typeof(VertexData)));

        int VertexOffset = 0;
        foreach(RenderCommand Command in Commands)
        {
            if(Command is RenderCommand_DrawPrimitive)
            {
                RenderCommand_DrawPrimitive CommandDraw = (RenderCommand_DrawPrimitive)Command;

                Device.DrawPrimitives(CommandDraw.PrimitiveType, VertexOffset, CommandDraw.PrimitiveCount);
                VertexOffset += CommandDraw.VertexCount;
            }
            else if(Command is RenderCommand_SetTexture)
            {
                RenderCommand_SetTexture CommandTex = (RenderCommand_SetTexture)Command;
                Device.SetTexture(0, CommandTex.Texture);
            }
        }
        
        Vertices.Clear();
        Commands.Clear();
        StateBlock.Apply();
    }

    public void Dispose()
    {
        this.Vertices = null;
        this.Commands = null;
        this.Device = null;
        this.StateBlock = null;
        this.Declaration = null;
        this.VertexBuffer = null;

        this.Initialized = false;
    }
    
    public float ScreenWidth {get {return Device.Viewport.Width;}}
    public float ScreenHeight {get {return Device.Viewport.Height;}}
}
