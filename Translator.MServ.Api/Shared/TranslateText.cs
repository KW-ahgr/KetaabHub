namespace Translator.MServ.Api.Shared
{
    public sealed record TranslateTextRequest(string Text, string ToLanguage);
    public sealed record TranslateTextResponse(string TranslatedText);
}
