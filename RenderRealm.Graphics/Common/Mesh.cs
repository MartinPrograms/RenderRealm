using Hexa.NET.OpenGL;
using System.Numerics;

namespace RenderRealm.Graphics.Common;

public class Mesh : IRenderer , IDisposable
{
    public Vertex[] Vertices { get; set; }
    public uint[] Indices { get; set; }
    
    public uint VAO { get; private set; }
    public uint VBO { get; private set; }
    public uint EBO { get; private set; }
    public static Mesh Square => new Mesh(new Vertex[]
    {
        new Vertex { Position = new Vector3(-1, -1, 0), TexCoord = new Vector2(0, 0) },
        new Vertex { Position = new Vector3(1, -1, 0), TexCoord = new Vector2(1, 0) },
        new Vertex { Position = new Vector3(1, 1, 0), TexCoord = new Vector2(1, 1) },
        new Vertex { Position = new Vector3(-1, 1, 0), TexCoord = new Vector2(0, 1) }
    }, new uint[]
    {
        0, 1, 2,
        2, 3, 0
    });

    public unsafe Mesh(Vertex[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;
        
        var Gl = GraphicsContext.GL;
        
        VAO = Gl.GenVertexArray();
        VBO = Gl.GenBuffer();
        EBO = Gl.GenBuffer();
        
        Gl.BindVertexArray(VAO);
        
        Gl.BindBuffer(GLBufferTargetARB.ArrayBuffer, VBO);
        
        float[] verticesData = new float[Vertices.Length * 5];
        for (int i = 0; i < Vertices.Length; i++)
        {
            verticesData[i * 5] = Vertices[i].Position.X;
            verticesData[i * 5 + 1] = Vertices[i].Position.Y;
            verticesData[i * 5 + 2] = Vertices[i].Position.Z;
            verticesData[i * 5 + 3] = Vertices[i].TexCoord.X;
            verticesData[i * 5 + 4] = Vertices[i].TexCoord.Y;
        }
        
        fixed (float* verticesPtr = verticesData)
        {
            Gl.BufferData(GLBufferTargetARB.ArrayBuffer, new IntPtr(sizeof(float) * verticesData.Length), verticesPtr, GLBufferUsageARB.StaticDraw);
        }
        
        Gl.BindBuffer(GLBufferTargetARB.ElementArrayBuffer, EBO);
        fixed (uint* indicesPtr = Indices)
        {
            Gl.BufferData(GLBufferTargetARB.ElementArrayBuffer, new IntPtr(sizeof(uint) * Indices.Length), indicesPtr, GLBufferUsageARB.StaticDraw);
        }
        
        Gl.EnableVertexAttribArray(0);
        Gl.VertexAttribPointer(0, 3, GLVertexAttribPointerType.Float, false, sizeof(float) * 5, 0);
        
        Gl.EnableVertexAttribArray(1);
        Gl.VertexAttribPointer(1, 2, GLVertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 3);
        
        Gl.BindVertexArray(0);
    }
    
    public unsafe void Render()
    {
        var Gl = GraphicsContext.GL;
        
        Gl.BindVertexArray(VAO);
        Gl.DrawElements(GLPrimitiveType.Triangles, Indices.Length, GLDrawElementsType.UnsignedInt, null);
        Gl.BindVertexArray(0);
    }
    
    public void Dispose()
    {
        GraphicsContext.GL.DeleteVertexArray(VAO);
        GraphicsContext.GL.DeleteBuffer(VBO);
        GraphicsContext.GL.DeleteBuffer(EBO);
    }
}

public class Vertex
{
    public Vector3 Position;
    public Vector2 TexCoord;
}