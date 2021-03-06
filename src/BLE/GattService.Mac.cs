﻿// SPDX-License-Identifier: MIT
// Copyright (c) 2021 David Lechner <david@lechnology.com>

// https://github.com/dotnet/roslyn/issues/54103
#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;

namespace Dandy.Devices.BLE
{
    partial class GattService
    {
        private readonly CBPeripheral peripheral;
        private readonly PeripheralDelegate @delegate;
        private readonly CBService service;

        internal GattService(CBPeripheral peripheral, PeripheralDelegate @delegate, Peripheral parent, CBService service)
        {
            this.peripheral = peripheral;
            this.@delegate = @delegate;
            Peripheral = parent;
            this.service = service;
        }

        private partial Guid GetUuid() => Platform.CBUuidToGuid(service.UUID);

        public async partial Task<IEnumerable<GattCharacteristic>> GetCharacteristicsAsync(IEnumerable<Guid>? uuids)
        {
            var cbUuids = uuids?.Select(x => CBUUID.FromString(x.ToString())).ToArray();
            var errorAwaiter = @delegate.DiscoveredCharacteristicObservable.FirstAsync(x => x.service == service).GetAwaiter();
            peripheral.DiscoverCharacteristics(cbUuids, service);
            var (_, error) = await errorAwaiter;

            if (error is not null) {
                throw new NSErrorException(error);
            }

            return service.Characteristics?.Select(x => new GattCharacteristic(peripheral, @delegate, this, x))
                ?? Enumerable.Empty<GattCharacteristic>();
        }
    }
}
