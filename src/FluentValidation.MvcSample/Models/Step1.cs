namespace FluentValidation.MvcSample.Models {
	using Attributes;

	[Validator(typeof(Step1Validator))]
	public class Step1 {
		public string Required { get; set; }
		public string Minimum { get; set; }
		public string Maximum { get; set; }
		public string MinMax { get; set; }
		public string Email { get; set; }
		public string CreditCard { get; set; }
		public string Url { get; set; }
		public string Number { get; set; }
		public string Digits { get; set; }
	}
}