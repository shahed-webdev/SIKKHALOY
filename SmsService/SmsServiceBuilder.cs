using System;

namespace SmsService
{
    public class SmsServiceBuilder
    {
        private readonly ISmsProvider _provider;
        public string Error { get; private set; }
        //public int StatusCode { get; private set; }
        public bool IsSuccess { get; private set; }

        public SmsServiceBuilder(ProviderEnum provider)
        {
            if (provider == ProviderEnum.BanglaPhone)
            {
                _provider = new SmsProviderBanglaPhone();
            }
            else if (provider == ProviderEnum.Mram)
            {
            }
            else if (provider == ProviderEnum.MimSms)
            {
            }

        }
        public int SmsBalance()
        {
            try
            {
                this.IsSuccess = true;
                return _provider.GetSmsBalance();
            }
            catch (Exception e)
            {
                this.IsSuccess = false;
                this.Error = e.Message;
            }
            return 0;

        }

        public string SendSms(string massage, string number)
        {
            try
            {
                this.IsSuccess = true;
                return _provider.SendSms(massage, number);
            }
            catch (Exception e)
            {
                this.Error = e.Message;
                this.IsSuccess = false;
            }

            return string.Empty;
        }
    }
}