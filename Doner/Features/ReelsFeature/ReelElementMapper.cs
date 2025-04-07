using Contracts.V1.Requests;
using Contracts.V1.Responses;
using Doner.Features.ReelsFeature.Elements;

namespace Doner.Features.ReelsFeature;

public static class ReelElementMapper
{
    public static ReelElement ToReelElement(this AddReelElementRequest addRequest) =>
        addRequest switch
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

    public static ReelElement ToReelElement(this UpdateReelElementRequest updateRequest) =>
        updateRequest switch
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

    public static ReelElementsResponse ToResponse(this IEnumerable<ReelElement> elements) =>
        new()
        {
            Items = elements.Select(e => e.ToResponse()).ToList()
        };

    public static ReelElementResponse ToResponse(this ReelElement element) =>
        element switch
        {
            Checkbox checkbox => new CheckboxResponse
            {
                Id = checkbox.Id,
                Header = checkbox.Header,
                IsChecked = checkbox.IsChecked
            },
            Dropdown dropdown => new DropdownResponse
            {
                Id = dropdown.Id,
                Elements = dropdown.Elements.Select(e => e.ToResponse()).ToList()
            },
            Picture picture => new PictureResponse
            {
                Id = picture.Id,
                Url = picture.Url,
                Width = picture.Width,
                Height = picture.Height,
                Caption = picture.Caption
            },
            PlainText plainText => new PlainTextResponse
            {
                Id = plainText.Id,
                Text = plainText.Text
            },
            _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
        };
}