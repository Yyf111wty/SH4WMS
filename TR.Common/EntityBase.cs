using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TR.Common
{
    /// <summary>
    ///     可持久到数据库的领域模型的基类。
    /// </summary>
    [Serializable]
    public abstract class EntityBase<Key>
    {
        protected EntityBase()
        {
            if (typeof(Key) == typeof(Guid))
            {
                Id = Guid.NewGuid().CastTo<Key>();
            }
            //IsDeleted = false;
            this.CreateTime = DateTime.Now;
            this.LastEditTime = DateTime.Now;
        }

        [Key]
        [Description("唯一标识")]
        public Key Id { get; set; }

        [Description("创建人")]
        [StringLength(20)]
        public string Creator { get; set; }

        [Description("创建时间")]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        ///     获取或设置 添加时间
        /// </summary>
        [Description("最后更新人")]
        [StringLength(20)]
        public string LastEditor { get; set; }

        /// <summary>
        ///     获取或设置 添加时间
        /// </summary>
        [Description("最后更新时间")]
        [DataType(DataType.DateTime)]
        public DateTime? LastEditTime { get; set; }
    }
}