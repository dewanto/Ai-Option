﻿using System.Collections.Generic;
using EventFlow.Queries;

namespace iqoption.domain.IqOption.Queries {
    public class GetIqOptionAccountByUserIdQuery : IQuery<IEnumerable<IqAccount>> {
        public GetIqOptionAccountByUserIdQuery(string userName) {
            UserName = userName;
        }

        public string UserName { get; }
    }

    public class InActiveAccountQuery : IQuery<IEnumerable<IqAccount>> {
    }

    public class ActiveAccountQuery : IQuery<IEnumerable<IqAccount>> {
    }
}