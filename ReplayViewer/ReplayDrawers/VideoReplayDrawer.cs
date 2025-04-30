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
    private readonly int _maxSizeBytes;
    
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
    private readonly SKFont _fontHeader = new()
    {
        Typeface = SKTypeface.FromFamilyName("Arial"),
        Size = 20,
    };
    private readonly SKFont _fontSmall = new()
    {
        Typeface = SKTypeface.FromFamilyName("Times New Roman"),
        Size = 8,
    };

    private int _width;
    private int _height;
    private readonly List<IVideoFrame> _frames = [];
    public Stream OutputStream { get; private set; }
    public int DrawnFramesCount => _frames.Count;

    public VideoReplayDrawer(bool exportAsFile, int maxSizeBytes = 0)
    {
        _exportAsFile = exportAsFile;
        _maxSizeBytes = maxSizeBytes;
    }

    public override float Speed => 1f;

    public override unsafe void DrawFrame(Frame f)
    {
        var stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);
        var tiles = f.StageTiles;
        _width = stage.TileDimensions.x * TileSizePixels;
        _height = stage.TileDimensions.y * TileSizePixels;
        
        var bmp = new SKBitmap(_width, _height);
        var g = new SKCanvas(bmp);
        g.Clear(SKColors.LightSkyBlue);
            
        g.DrawText($"{DrawnFramesCount}", 2, 20, _fontHeader, _textPaint);
        
        if (tiles == null) return;
        for (var x = 0; x < stage.TileDimensions.x; x++) {
            for (var y = 0; y < stage.TileDimensions.y; y++) {
                var tile = tiles[x + y * stage.TileDimensions.x];
                if (tile.HasWorldPolygons(f))
                {
                    var tileAsset = (StageTile) Replay.ResourceManager.GetAsset(tile.Tile.Id);
                    foreach (var shape in tileAsset.CollisionData.Shapes)
                    {
                        var path = new SKPath();
                        foreach (var point in shape.Vertices)
                        {
                            var translatedPoint = new FPVector2(point.X + x * TileSizePixels, point.Y + y * TileSizePixels);
                            if (path.Points.Length == 0) path.MoveTo(translatedPoint.X.AsFloat, translatedPoint.Y.AsFloat);
                            else path.LineTo(translatedPoint.X.AsFloat, translatedPoint.Y.AsFloat);
                        }
                        g.DrawPath(path, _tilePaint);
                    }
                    // g.DrawRect(x * TileSizePixels, -(y * TileSizePixels) + _height, TileSizePixels, TileSizePixels, _tilePaint);
                }
            }
        }
        
        var marios = f.Filter<MarioPlayer>();
        marios.UseCulling = false;
        while (marios.NextUnsafe(out var entity, out var mario))
        {
            f.Unsafe.TryGetPointer(entity, out Transform2D* transform);
            RuntimePlayer player = f.GetPlayerData(mario->PlayerRef);
            var pos = WorldToRelativeTileSmooth(stage, transform->Position, TileSizePixels);
            g.DrawCircle(pos.X.AsFloat, pos.Y.AsFloat, TileSizePixels / 4.0f, _marioPaint);
            g.DrawText(player.PlayerNickname, pos.X.AsFloat, pos.Y.AsFloat + 10, _fontSmall, _textPaint);
        }
        
        // var all = f.Filter<Transform2D>();
        // all.UseCulling = false;
        // while (all.NextUnsafe(out _, out var transform)) {
        //     var posA = QuantumUtils.WorldToRelativeTile(f, transform->Position);
        //     g.DrawCircle(posA.x * TileSizePixels, -(posA.y * TileSizePixels) + _height, TileSizePixels / 4.0f, _marioPaint);
        //     var posB = WorldToRelativeTileSmooth(stage, transform->Position);
        //     g.DrawCircle(posB.X.AsFloat * TileSizePixels, -(posB.Y.AsFloat * TileSizePixels) + _height, TileSizePixels / 4.0f, _marioPaint);
        // }
        
        var powerups = f.Filter<Powerup>();
        powerups.UseCulling = false;
        while (powerups.NextUnsafe(out var entity, out var powerup))
        {
            f.Unsafe.TryGetPointer(entity, out Transform2D* transform);
            var pos = WorldToRelativeTileSmooth(stage, transform->Position, TileSizePixels);
            g.DrawCircle(pos.X.AsFloat, pos.Y.AsFloat, TileSizePixels / 4.0f, _powerupPaint);
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
    
    public static FPVector2 WorldToRelativeTileSmooth(VersusStageData stage, FPVector2 worldPos, int magnify = 1) {
        worldPos -= stage.TilemapWorldPosition;
        worldPos *= 2;
        worldPos -= new FPVector2(stage.TileOrigin.x, stage.TileOrigin.y);
        // worldPos *= magnify;
        return QuantumUtils.WrapWorld(stage, worldPos, out _);
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
