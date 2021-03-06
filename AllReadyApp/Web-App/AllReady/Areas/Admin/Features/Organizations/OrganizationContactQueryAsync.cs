﻿using AllReady.Areas.Admin.Models;
using AllReady.Models;
using MediatR;

namespace AllReady.Areas.Admin.Features.Organizations
{
    public class OrganizationContactQueryAsync : IAsyncRequest<ContactInformationModel>
    {
        public int OrganizationId { get; set; }
        public ContactTypes ContactType { get; set; } = ContactTypes.Primary;
    }
}