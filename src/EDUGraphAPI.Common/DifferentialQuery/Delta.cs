/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EDUGraphAPI.DifferentialQuery
{
    public class Delta<TEntity> : IDeltaEntity where TEntity : class
    {
        public Delta(TEntity entity)
        {
            this.Entity = entity;
        }

        public TEntity Entity { get; private set; }

        [JsonProperty("@removed")]
        public DeltaRemovedData Removed { get; set; }

        public HashSet<string> ModifiedPropertyNames { get; set; }

        public bool IsRemoved => this.Removed != null;
    }
}