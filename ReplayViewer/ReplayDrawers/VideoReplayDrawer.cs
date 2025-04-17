using System.Diagnostics;
using FFMpegCore;
using FFMpegCore.Pipes;
using Photon.Deterministic;
using Quantum;
using SkiaSharp;

namespace ReplayViewer.ReplayDrawers;

public class VideoReplayDrawer : ReplayDrawer
{
    private const int TileSizePixels = 8;
    
    private readonly bool _exportAsFile;
    
    private readonly SKPaint _marioPaint = new()
    {
        Color = SKColors.Green,
        Style = SKPaintStyle.Fill,
    };
    private readonly SKPaint _powerupPaint = new()
    {
        Color = SKColors.Red,
        Style = SKPaintStyle.Fill,
    };
    private readonly SKPaint _tilePaint = new()
    {
        Color = SKColors.Gray,
        Style = SKPaintStyle.Stroke,
    };
    private readonly SKPaint _textPaint = new()
    {
        Color = SKColors.White,
        Style = SKPaintStyle.Fill,
    };
    private readonly SKFont _font = new()
    {
        Typeface = SKTypeface.FromFamilyName("Arial"),
        Size = 20,
    };

    private int _width;
    private int _height;
    private readonly List<IVideoFrame> _frames = [];
    public Stream OutputStream { get; private set; }
    public int DrawnFramesCount => _frames.Count;

    public VideoReplayDrawer(bool exportAsFile)
    {
        _exportAsFile = exportAsFile;
    }

    public override unsafe void DrawFrame(Frame f)
    {
        var stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);
        var tiles = f.StageTiles;
        _width = stage.TileDimensions.x * TileSizePixels;
        _height = stage.TileDimensions.y * TileSizePixels;
        var stageCenter = (stage.StageWorldMax - stage.StageWorldMin) / 2;
        
        var bmp = new SKBitmap(_width, _height);
        var g = new SKCanvas(bmp);
        g.Clear(SKColors.LightSkyBlue);
            
        g.DrawText($"{DrawnFramesCount}", 2, 20, _font, _textPaint);
        
        if (tiles == null) return;
        for (var x = 0; x < stage.TileDimensions.x; x++) {
            for (var y = 0; y < stage.TileDimensions.y; y++) {
                if (tiles[x + y * stage.TileDimensions.x].HasWorldPolygons(f)) g.DrawRect(x * TileSizePixels, -(y * TileSizePixels) + _height, TileSizePixels, TileSizePixels, _tilePaint);
            }
        }

        var marios = f.Filter<MarioPlayer>();
        marios.UseCulling = false;
        while (marios.NextUnsafe(out var entity, out var mario))
        {
            f.Unsafe.TryGetPointer(entity, out Transform2D* transform);
            var pos = transform->Position + stageCenter;
            g.DrawCircle(pos.X.AsFloat * TileSizePixels * 2, -(pos.Y.AsFloat * TileSizePixels) + _height, TileSizePixels / 4.0f, _marioPaint);
        }
        
        var powerups = f.Filter<Powerup>();
        powerups.UseCulling = false;
        while (powerups.NextUnsafe(out var entity, out var powerup))
        {
            f.Unsafe.TryGetPointer(entity, out Transform2D* transform);
            var pos = transform->Position + stageCenter;
            g.DrawCircle(pos.X.AsFloat * TileSizePixels * 2, -(pos.Y.AsFloat * TileSizePixels) + _height, TileSizePixels / 4.0f, _powerupPaint);
        }

        _frames.Add(new SKBitmapFrame(bmp));
    }

    public override void Render()
    {
        RawVideoPipeSource source = new(_frames)
        {
            FrameRate = 60,
        };
        
        if (_exportAsFile)
        {
            FFMpegArguments
                .FromPipeInput(source)
                .OutputToFile("out.mp4", overwrite: true, options => options.WithVideoCodec("libx264").WithVideoBitrate(5000))
                .ProcessSynchronously();
        }
        else
        {
            OutputStream = new MemoryStream();
            FFMpegArguments
                .FromPipeInput(source)
                .OutputToPipe(new StreamPipeSink(OutputStream), options => options.WithVideoCodec("libx264").ForceFormat("mp4").WithCustomArgument("-movflags frag_keyframe+empty_moov"))
                .ProcessSynchronously();
            OutputStream.Position = 0;
        }
    }
    
    class SKBitmapFrame(SKBitmap bmp) : IVideoFrame, IDisposable
    {
        public int Width => bmp.Width;
        public int Height => bmp.Height;
        public string Format => "bgra";

        public void Dispose() => bmp.Dispose();

        public void Serialize(Stream pipe) => pipe.Write(bmp.Bytes, 0, bmp.Bytes.Length);

        public Task SerializeAsync(Stream pipe, CancellationToken token) => pipe.WriteAsync(bmp.Bytes, 0, bmp.Bytes.Length, token);
    }
}
