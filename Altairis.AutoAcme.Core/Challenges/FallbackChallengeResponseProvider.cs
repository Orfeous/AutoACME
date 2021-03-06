﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Certes.Acme;

namespace Altairis.AutoAcme.Core.Challenges {
    public class FallbackChallengeResponseProvider : IChallengeResponseProvider {
        private readonly ChallengeResponseProvider[] providers;
        private int index;

        public FallbackChallengeResponseProvider(params ChallengeResponseProvider[] providers) { this.providers = providers; }

        public void Dispose() => Array.ForEach(this.providers, provider => provider.Dispose());

        public Task<bool> ValidateAsync(AutoAcmeContext context, IEnumerable<IAuthorizationContext> authorizationContexts) {
            if (this.index >= this.providers.Length) {
                return Task.FromResult(false);
            }
            var provider = this.providers[this.index];
            Log.WriteLine("Validate via " + provider.ChallengeType + "...");
            Log.Indent();
            try {
                return provider.ValidateAsync(context, authorizationContexts);
            }
            finally {
                Log.Unindent();
            }
        }

        public async Task<bool> TestAsync(IEnumerable<string> hostNames) {
            this.index = 0;
            while (this.index < this.providers.Length) {
                var provider = this.providers[this.index];
                Log.WriteLine($"Testing {provider.ChallengeType}...");
                Log.Indent();
                try {
                    if (await provider.TestAsync(hostNames).ConfigureAwait(true)) {
                        return true;
                    }
                }
                finally {
                    Log.Unindent();
                }
                this.index++;
            }
            return false;
        }
    }
}
