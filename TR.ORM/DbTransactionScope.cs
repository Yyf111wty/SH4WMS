using System;

namespace TR.ORM
{
    /// <summary>
    /// 数据库事务作用域。未调用 Commit 即 Dispose 时自动回滚，
    /// 避免提前 return / 异常路径把事务悬挂在共享连接上。
    /// </summary>
    public sealed class DbTransactionScope : IDisposable
    {
        private bool _completed;

        private DbTransactionScope()
        {
            SqlSugarClientFactory.BeginTran();
        }

        public static DbTransactionScope Begin()
        {
            return new DbTransactionScope();
        }

        public void Commit()
        {
            SqlSugarClientFactory.CommitTran();
            _completed = true;
        }

        public void Rollback()
        {
            SqlSugarClientFactory.RollbackTran();
            _completed = true;
        }

        public void Dispose()
        {
            if (_completed)
            {
                return;
            }

            SqlSugarClientFactory.RollbackTran();
        }
    }
}
