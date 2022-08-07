using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading.Utilities
{
    internal class TranslationHelper
    {
        private ITranslationHelper _translations;

        public string TradeRequest => _translations.Get("RequestMessage");
        public string TradeAccept => _translations.Get("AcceptRequest");
        public string TradeDecline => _translations.Get("DeclineRequest");
        public string TradeDeclined => _translations.Get("RequestDeclined");
        public string PendingResponse => _translations.Get("PendingResponse");

        public string AcceptOffer => _translations.Get("AcceptOffer");
        public string SendOffer => _translations.Get("SendOffer");
        public string ConfirmOffer => _translations.Get("Confirm");

        public TranslationHelper(IModHelper helper) => _translations = helper.Translation;
    }
}
