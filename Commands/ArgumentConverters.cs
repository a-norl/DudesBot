using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using ImageMagick;

public class MagickImageConverter : IArgumentConverter<MagickImage>
{
    public Task<Optional<MagickImage>> ConvertAsync(string value, CommandContext ctx)
    {
        throw new System.NotImplementedException();
    }
}