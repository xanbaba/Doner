using Contracts.V1.Requests;
using Doner.Features.ReelsFeature.Elements;

namespace Doner.Features.ReelsFeature;

public static class ReelElementMapper
{
    public static ReelElement ToReelElement(this AddReelElementRequest addRequest)
    {
        return addRequest switch
        {
            AddCheckboxRequest checkboxRequest => new Checkbox
            {
                Id = Guid.CreateVersion7(),
                Header = checkboxRequest.Header ?? string.Empty,
                IsChecked = checkboxRequest.IsChecked
            },
            AddPlainTextRequest plainTextRequest => new PlainText
            {
                Id = Guid.CreateVersion7(),
                Text = plainTextRequest.Text
            },
            AddPictureRequest pictureRequest => new Picture
            {
                Id = Guid.CreateVersion7(),
                Url = pictureRequest.Url,
                Width = pictureRequest.Width,
                Height = pictureRequest.Height
            },
            AddDropdownRequest dropdownRequest => new Dropdown
            {
                Id = Guid.CreateVersion7(),
                Elements = dropdownRequest.Elements.Select(e => e.ToReelElement()).ToList()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(addRequest), addRequest, null)
        };
    }
    
    public static ReelElement ToReelElement(this UpdateReelElementRequest updateRequest)
    {
        return updateRequest switch
        {
            UpdateCheckboxRequest checkboxRequest => new Checkbox
            {
                Id = Guid.CreateVersion7(),
                Header = checkboxRequest.Header ?? string.Empty,
                IsChecked = checkboxRequest.IsChecked
            },
            UpdatePlainTextRequest plainTextRequest => new PlainText
            {
                Id = Guid.CreateVersion7(),
                Text = plainTextRequest.Text
            },
            UpdatePictureRequest pictureRequest => new Picture
            {
                Id = Guid.CreateVersion7(),
                Url = pictureRequest.Url,
                Width = pictureRequest.Width,
                Height = pictureRequest.Height
            },
            UpdateDropdownRequest dropdownRequest => new Dropdown
            {
                Id = Guid.CreateVersion7(),
                Elements = dropdownRequest.Elements.Select(e => e.ToReelElement()).ToList()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(updateRequest), updateRequest, null)
        };
    }
}