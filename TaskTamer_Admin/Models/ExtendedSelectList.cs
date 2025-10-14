using System.Collections;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskTamer_Admin.Models;

public class ExtendedSelectListItem : SelectListItem
{
    public string DescriptionField { get; set; } = string.Empty;
    public string DescriptionField2 { get; set; } = string.Empty;


    public ExtendedSelectListItem() : base() { }

    public ExtendedSelectListItem(string dataValueField, string dataTextField)
        : base(dataValueField, dataTextField) { }

    public ExtendedSelectListItem( string dataValueField, string dataTextField, bool selectedValue)
        : base(dataValueField, dataTextField, selectedValue) { }


    public ExtendedSelectListItem(IEnumerable items, string dataValueField, string dataTextField,
        string descriptionField, string descriptionField2)
        : base( dataValueField, dataTextField)
    {
        DescriptionField = descriptionField;
        DescriptionField2 = descriptionField2;

    }

}