﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Smartstore.Web.Modelling;

namespace Smartstore.Admin.Models.Directory
{
    public class MeasureWeightModel : EntityModelBase, ILocalizedModel<MeasureWeightLocalizedModel>
    {
        [LocalizedDisplay("Admin.Configuration.Measures.Weights.Fields.Name")]
        public string Name { get; set; }

        [LocalizedDisplay("Admin.Configuration.Measures.Weights.Fields.SystemKeyword")]
        public string SystemKeyword { get; set; }

        [LocalizedDisplay("Admin.Configuration.Measures.Weights.Fields.Ratio")]
        [UIHint("Decimal")]
        [AdditionalMetadata("decimals", 8)]
        public decimal Ratio { get; set; }

        [LocalizedDisplay("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [LocalizedDisplay("Admin.Configuration.Measures.Weights.Fields.IsPrimaryWeight")]
        public bool IsPrimaryWeight { get; set; }

        public List<MeasureWeightLocalizedModel> Locales { get; set; } = new();
    }

    public class MeasureWeightLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [LocalizedDisplay("Admin.Configuration.Measures.Weights.Fields.Name")]
        public string Name { get; set; }
    }

    public partial class MeasureWeightValidator : AbstractValidator<MeasureWeightModel>
    {
        public MeasureWeightValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.SystemKeyword).NotEmpty();
        }
    }
}