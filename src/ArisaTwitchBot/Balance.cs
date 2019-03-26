using ArisaTwitchBot.Services.Database;
using System;

namespace ArisaTwitchBot
{
    public class Balance
    {
        public readonly string UserId;
        private readonly UserService _userService;
        private readonly RefLong _balance;

        public Balance(string userId, long balance, UserService userService)
        {
            UserId = userId;
            _balance = new RefLong(balance);
            _userService = userService;
        }

        public long ReadUnsafe() => _balance.Value;

        public void ExecuteTransaction(Action<RefLong> action)
            => ExecuteTransaction(_ => true, ifTrue: action);

        public bool ExecuteTransaction(Func<long, bool> condition, Action<RefLong> ifTrue, Action<RefLong> ifFalse = null)
        {
            lock (_balance)
            {
                bool conditionResult = condition(_balance.Value);

                RefLong dummyBalance = new RefLong(_balance.Value);

                if (conditionResult)
                {
                    ifTrue(dummyBalance);
                }
                else
                {
                    ifFalse?.Invoke(dummyBalance);
                }

                if (_balance.Value != dummyBalance.Value)
                {
                    _balance.Value = dummyBalance.Value;
                    _userService.SaveChanges(UserId, _balance.Value);
                }

                return conditionResult;
            }
        }
    }
}
