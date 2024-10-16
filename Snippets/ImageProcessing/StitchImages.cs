/* Stitch Images
Version: v0.2
Last Update: 2023 Nov

Performs the image presentation trick as often shown in Houdini, but programmed in C#, with programmably adjustable parameters.

Notice Zora (at the moment inside ImageProcessing library) has an implementation for this as well.

Changelog:
* Version 0.1: Initial setup.
* Version 0.2: Expose caption configurations.
*/
Import(Magick.NET-Q8-AnyCPU)

using System.Drawing;
public class Parameters
{
    public int Margin = 20;
    public int ImagesPerRow = 2;
    public Color BackgroundColor = Color.Black;
    public Size Crop = new Size(1920, 1080);

    public double FontSize = 52.0;
    public MagickColor TextColor = MagickColors.White;
    public Size CaptionBox = new Size(680, 250);
}

private static String ToHex(Color c)
	=> "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

public void Stitch(string sourceFolder, string outputFilePath, Parameters parameters)
{
    var files = Directory.EnumerateFiles(sourceFolder).ToArray();

    int rows = (int)Math.Ceiling((double)files.Length / parameters.ImagesPerRow);
    var composition = new MagickImage(
        new MagickColor(ToHex(parameters.BackgroundColor)), 
        parameters.ImagesPerRow * parameters.Crop.Width + (parameters.ImagesPerRow + 1) * parameters.Margin, 
        rows * parameters.Crop.Height + (rows + 1) * parameters.Margin
    );

    for(int i = 0; i < files.Length; i++)
    {
        string file = files[i];
        var image = new MagickImage(file);
        image.Crop(new MagickGeometry(image.Width / 2 - parameters.Crop.Width / 2, image.Height / 2 - parameters.Crop.Height / 2, parameters.Crop.Width, parameters.Crop.Height));
        new Drawables()
            // Add a border
            .StrokeColor(new MagickColor(0, Quantum.Max, 0))
            .StrokeWidth(2)
            .FillColor(MagickColors.Transparent)
            .Rectangle(0, 0, parameters.Crop.Width, parameters.Crop.Height)
            .Draw(image);
        
        // Add caption
        var captionSettings = new MagickReadSettings
        {
            Font = "Calibri",
            FontPointsize = parameters.FontSize,
            TextGravity = Gravity.Center,
            BackgroundColor = MagickColors.Transparent,
            Height = parameters.CaptionBox.Height, // height of text box
            Width = parameters.CaptionBox.Width, // width of text box
            FillColor = parameters.TextColor
        };
        var caption = new MagickImage($"Caption:{Path.GetFileNameWithoutExtension(file)}", captionSettings);
        image.Composite(caption, parameters.Margin, parameters.Margin, CompositeOperator.Over);
        
        // Composite
        int row = i / parameters.ImagesPerRow;
        int col = i % parameters.ImagesPerRow;
        composition.Composite(image, col * parameters.Crop.Width + (col + 1) * parameters.Margin, row * parameters.Crop.Height + (row + 1) * parameters.Margin, CompositeOperator.Over);	
    }
        
    composition.Write(outputFilePath);
}

// Doc
WriteLine("""
Type:
  class Parameters(int Margin = 20, int ImagesPerRow = 2, Color BackgroundColor = Color.Black, Size Crop = new Size(1920, 1080))

Method:
  void Stitch(string sourceFolder, string outputFilePath, Parameters parameters)

Example:
  Stitch(@"Input Folder", @"OutputPath.png", new Parameters(){
	TextColor = MagickColors.Black,
	CaptionBox = new Size(680, 250)
  })
""");