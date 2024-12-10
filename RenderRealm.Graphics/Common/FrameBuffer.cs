using Hexa.NET.OpenGL;

namespace RenderRealm.Graphics.Common;

public class FrameBuffer : IDisposable
{
    public uint Id { get; private set; }
    public uint TextureId { get; private set; }
    public uint DepthId { get; private set; }
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public unsafe FrameBuffer(int width, int height)
    {
        Width = width;
        Height = height;

        var Gl = GraphicsContext.GL;
        
        Id = Gl.GenFramebuffer();
        Gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, Id);
        
        TextureId = Gl.GenTexture();
        Gl.BindTexture(GLTextureTarget.Texture2D, TextureId);
        Gl.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba, width, height, 0, GLPixelFormat.Rgba, GLPixelType.UnsignedByte, null);
        Gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)GLTextureMinFilter.Linear);
        Gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)GLTextureMagFilter.Linear);
        Gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.WrapS, (int)GLTextureWrapMode.ClampToEdge);
        Gl.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.WrapT, (int)GLTextureWrapMode.ClampToEdge);
        Gl.FramebufferTexture2D(GLFramebufferTarget.Framebuffer, GLFramebufferAttachment.ColorAttachment0, GLTextureTarget.Texture2D, TextureId, 0);
        
        DepthId = Gl.GenRenderbuffer();
        Gl.BindRenderbuffer(GLRenderbufferTarget.Renderbuffer, DepthId);
        Gl.RenderbufferStorage(GLRenderbufferTarget.Renderbuffer, GLInternalFormat.Depth24Stencil8, width, height);
        Gl.FramebufferRenderbuffer(GLFramebufferTarget.Framebuffer, GLFramebufferAttachment.DepthStencilAttachment, GLRenderbufferTarget.Renderbuffer, DepthId);
        
        Gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);
    }
    
    public void Bind()
    {
        GraphicsContext.GL.BindFramebuffer(GLFramebufferTarget.Framebuffer, Id);
        GraphicsContext.GL.Viewport(0, 0, Width, Height);
    }
    
    public void Unbind()
    {
        GraphicsContext.GL.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);
        GraphicsContext.GL.Viewport(0, 0, GraphicsContext.Width, GraphicsContext.Height);
    }
    
    public void Dispose()
    {
        GraphicsContext.GL.DeleteFramebuffer(Id);
        GraphicsContext.GL.DeleteTexture(TextureId);
        GraphicsContext.GL.DeleteRenderbuffer(DepthId);
    }

    public unsafe void Resize(int sizeX, int sizeY)
    {
        if (sizeX == Width && sizeY == Height)
            return;
        
        Width = sizeX;
        Height = sizeY;
        
        var Gl = GraphicsContext.GL;
        
        Gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, Id);
        
        Gl.BindTexture(GLTextureTarget.Texture2D, TextureId);
        Gl.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba, sizeX, sizeY, 0, GLPixelFormat.Rgba, GLPixelType.UnsignedByte, null);
        
        Gl.BindRenderbuffer(GLRenderbufferTarget.Renderbuffer, DepthId);
        Gl.RenderbufferStorage(GLRenderbufferTarget.Renderbuffer, GLInternalFormat.Depth24Stencil8, sizeX, sizeY);
        
        Gl.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);
    }
}