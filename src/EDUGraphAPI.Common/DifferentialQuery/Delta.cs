/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDUGraphAPI.DifferentialQuery
{
    public class Delta<TEntity> where TEntity : class
    {
        private static PropertyInfo EntityIdProperty { get; } = typeof(TEntity).GetProperty("Id",
            BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);

        public Delta(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; }

        [JsonProperty("@removed")]
        public DeltaRemovedData Removed { get; set; }

        public Dictionary<string, JToken> ModifiedProperties { get; set; } =
            new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);

        public bool IsRemoved => this.Removed != null;

        public string Id
        {
            get { return this.Entity == null ? null : EntityIdProperty.GetValue(this.Entity) as string; }
            set
            {
                if (this.Entity != null)
                {
                    EntityIdProperty.SetValue(this.Entity, value);
                }
            }
        }
    }
}
