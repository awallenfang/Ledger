using Prometheus;
using SkiaSharp;

public class RankCardService : IDisposable
{
    private static readonly Histogram TaskDuration = Metrics.CreateHistogram(
    "rank_card_rendering", "Time to render rank cards");
    private static int Width = 600;
    private static int Height = 200;
    private SKTypeface montserrat = SKTypeface.FromFile("/fonts/Montserrat-VariableFont_wght.ttf");

    private SKPaint bgPaint = new SKPaint { Color = new SKColor(15, 8, 32), IsAntialias = true };
    private SKPaint bgAltPaint = new SKPaint { Color = new SKColor(28, 17, 51), IsAntialias = true };
    private SKPaint accentPaint = new SKPaint { Color = new SKColor(80, 22, 68), IsAntialias = true };
    private SKPaint textPaint = new SKPaint { Color = new SKColor(255, 255, 255), IsAntialias = true };
    private SKRoundRect bgRect;
    private SKRoundRect barBgRect = new SKRoundRect(new SKRect(25, 155, 575, 175), 5);
    private SKPoint profilePoint = new SKPoint(75, 75);
    private SKPoint namePoint = new SKPoint(150, 50);
    private SKPoint levelPoint = new SKPoint(150, 100);
    private SKPoint expPoint = new SKPoint(300, 140);
    private SKPoint rankPoint = new SKPoint(400, 100);
    private SKFont nameFont;
    private SKFont levelFont;
    private SKFont expFont;
    private SKFont rankFont;
    private SKRect avatarRect;

    public RankCardService()
    {
        bgRect = new SKRoundRect(new SKRect(0, 0, Width, Height), 20);
        nameFont = new SKFont(montserrat, 32);
        levelFont = new SKFont(montserrat, 24);
        expFont = new SKFont(montserrat, 18);
        rankFont = new SKFont(montserrat, 40);
        avatarRect = new SKRect(profilePoint.X - 45, profilePoint.Y - 45, profilePoint.X + 45, profilePoint.Y + 45);
    }

    public void Dispose()
    {
        bgRect.Dispose();
        bgPaint.Dispose();
        bgAltPaint.Dispose();
        accentPaint.Dispose();
        textPaint.Dispose();
        barBgRect.Dispose();
        nameFont.Dispose();
        levelFont.Dispose();
        expFont.Dispose();
        rankFont.Dispose();
        // montserrat.Dispose();
    }

    public byte[] GenerateRankCard(RankCardData data)
    {
        using (TaskDuration.NewTimer())
        {
            var bar_percentage = (float)(data.CurrentXp - data.PrevLevel) / (float)(data.CurrentXp + data.NextLevel - data.PrevLevel);
            using var barRect = new SKRoundRect(new SKRect(30, 160, 30 + 540 * bar_percentage, 170), 5);
            var imageInfo = new SKImageInfo(Width, Height);
            using var surface = SKSurface.Create(imageInfo);


            var canvas = surface.Canvas;
            canvas.DrawRoundRect(bgRect, bgPaint);
            canvas.DrawRoundRect(barBgRect, bgAltPaint);
            canvas.DrawRoundRect(barRect, accentPaint);
            canvas.DrawText(data.Username, namePoint, SKTextAlign.Left, nameFont, textPaint);
            canvas.DrawText($"Level {data.Level}", levelPoint, SKTextAlign.Left, levelFont, textPaint);
            canvas.DrawText($"{data.CurrentXp} / {data.CurrentXp + data.NextLevel}", expPoint, SKTextAlign.Center, expFont, textPaint);
            canvas.DrawText($"#{data.Position}", rankPoint, SKTextAlign.Left, rankFont, textPaint);
            if (data.AvatarBitmap is not null)
            {
                var cropped_size = Math.Min(data.AvatarBitmap.Width, data.AvatarBitmap.Height);
                var xOffset = data.AvatarBitmap.Width - cropped_size;
                var yOffset = data.AvatarBitmap.Height - cropped_size;
                var cropped = new SKBitmap(cropped_size, cropped_size);
                data.AvatarBitmap.ExtractSubset(cropped, new SKRectI(xOffset, yOffset, xOffset + cropped_size, yOffset + cropped_size));
                canvas.DrawCircle(profilePoint, 50, accentPaint);

                float scale = avatarRect.Width / (float)cropped_size;

                var matrix = SKMatrix.CreateTranslation(avatarRect.Left, avatarRect.Top);
                matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(scale, scale));

                using (var paint = new SKPaint())
                using (var shader = SKShader.CreateBitmap(cropped, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, matrix))
                {
                    paint.Shader = shader;
                    paint.IsAntialias = true;

                    float cornerRadius = 100f;

                    canvas.DrawRoundRect(avatarRect, cornerRadius, cornerRadius, paint);
                }

            }

            using var image = surface.Snapshot();

            using var pngData = image.Encode(SKEncodedImageFormat.Png, 100);
            return pngData.ToArray();
        }
    }
}

public class RankCardData
{
    public string Username { get; set; } = "";
    public int Level { get; set; }
    public long CurrentXp { get; set; }
    public long NextLevel { get; set; }
    public long PrevLevel { get; set; }
    public int Position { get; set; }
    public SKBitmap? AvatarBitmap { get; set; }
}