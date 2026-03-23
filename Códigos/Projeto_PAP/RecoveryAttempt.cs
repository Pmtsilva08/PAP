using System;

namespace Projeto_PAP
{
    public class RecoveryAttempt
    {
        public DateTime? LastFailedAttempt { get; set; }
        public DateTime? LastSuccessfulRecovery { get; set; }
    }
}   