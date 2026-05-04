using System.Collections.Generic;

namespace Sunmax.GridAssetSlicer
{
    public static class ExportSettingsValidator
    {
        public static IReadOnlyList<string> Validate(ExportSettings settings)
        {
            var errors = new List<string>();
            Validate(settings, errors);
            return errors;
        }

        public static void Validate(ExportSettings settings, ICollection<string> errors)
        {
            if (settings == null)
            {
                errors.Add("Export settings are required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.OutputFolder))
            {
                errors.Add("Output Folder is required.");
            }

            if (settings.FilePrefix == null)
            {
                errors.Add("Output Prefix is required.");
            }

            if (settings.StartIndex < 0)
            {
                errors.Add("Start Index must be zero or greater.");
            }

            if (settings.NumberPadding < 0)
            {
                errors.Add("Serial Digits must be zero or greater.");
            }

            var hasOutputWidth = settings.OutputWidth.HasValue;
            var hasOutputHeight = settings.OutputHeight.HasValue;
            if (hasOutputWidth != hasOutputHeight)
            {
                errors.Add("Output Width and Output Height must both be specified together.");
                return;
            }

            if (hasOutputWidth && settings.OutputWidth.Value <= 0)
            {
                errors.Add("Output Width must be greater than zero when specified.");
            }

            if (hasOutputHeight && settings.OutputHeight.Value <= 0)
            {
                errors.Add("Output Height must be greater than zero when specified.");
            }
        }
    }
}
