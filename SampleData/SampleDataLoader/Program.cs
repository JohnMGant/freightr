using System.Text;
using System.Text.RegularExpressions;

internal class Program
{

    private static readonly Random random = new();

    private static void Main(string[] args)
    {        
        const int numberOfOrders = 500;

        List<Order> orders = new();
        List<OrderStop> orderStops = new();
        List<OrderStopCargoChange> orderStopCargoChanges = new();
        List<OrderAdditionalCharge> orderAdditionalCharges = new();

        DateTime orderDate = DateTime.Now.Subtract(TimeSpan.FromDays(numberOfOrders + 30));

        var customers = new[]
        {
            new {CustomerId = "000001", IsPayer = true},
            new {CustomerId = "000002", IsPayer = false},
            new {CustomerId = "000003", IsPayer = true},
            new {CustomerId = "000004", IsPayer = true},
            new {CustomerId = "000005", IsPayer = true},
            new {CustomerId = "000006", IsPayer = false},
            new {CustomerId = "000007", IsPayer = true},
            new {CustomerId = "000008", IsPayer = false},
            new {CustomerId = "000009", IsPayer = true},
            new {CustomerId = "000010", IsPayer = false},
        };
        string[] payers = customers.Where(c => c.IsPayer).Select(c => c.CustomerId).ToArray();

        var carriers = new[]
        {
            new {CarrierId = "000001", LoadTypes = new[] {"Dry Van", "Refrigerated", "Hazmat"}},
            new {CarrierId = "000002", LoadTypes = new[] {"Dry Van", "LTL"}},
            new {CarrierId = "000003", LoadTypes = new[] {"Refrigerated"}},
            new {CarrierId = "000004", LoadTypes = new[] {"Dry Van", "Hazmat", "Flatbed"}},
            new {CarrierId = "000005", LoadTypes = new[] {"Dry Van"}}
        };

        for (int orderIndex = 0; orderIndex < numberOfOrders; orderIndex++)
        {
            string orderId = $"{orderIndex + 1}".PadLeft(12, '0');
            var carrier = carriers[random.Next(0, carriers.Length - 1)];
            DateTime orderPlacedDate = orderDate;
            orderDate = orderDate.AddDays(1);
            string orderStatus = GetOrderStatus(orderPlacedDate);
            DateTime? orderClosedDate =
                (orderStatus == "Final" || orderStatus == "Canceled")
                ? orderPlacedDate.AddDays(random.Next(3, 21))
                : null;
            orders.Add(new Order
            {
                OrderId = orderId,
                BillOfLadingNumber = $"{random.NextInt64(100_000, 100_000_000_000)}",
                PayerCustomerId = payers[random.Next(0, payers.Length - 1)],
                CustomerReferenceNumber = GetRandomGeneralIdentifier(),
                CarrierId = carrier.CarrierId,
                OrderPlacedDate = orderPlacedDate,
                OrderStatus = orderStatus,
                OrderClosedDate = orderClosedDate,
                LoadType = carrier.LoadTypes[random.Next(0, carrier.LoadTypes.Length - 1)]
            });

            int numberOfExtraPickups = GetWeightedStopCount();
            int numberOfExtraDropoffs = GetWeightedStopCount();
            int numberOfReloads = GetWeightedStopCount();
            int numberOfOtherStops = GetWeightedStopCount();
            string[] extraStopTypes =
                Enumerable.Repeat("Pickup", numberOfExtraPickups)
                .Concat(Enumerable.Repeat("Dropoff", numberOfExtraDropoffs))
                .Concat(Enumerable.Repeat("Other cargo change", numberOfReloads))
                .Concat(Enumerable.Repeat("Carrier-related", numberOfOtherStops))
                .OrderBy(n => random.Next())
                .ToArray();
            string[] allStopTypes = new[] { "Pickup" }.Concat(extraStopTypes).Append("Dropoff").ToArray();
            TimeSpan dateRange = (orderClosedDate ?? orderPlacedDate.AddDays(random.Next(5, 30))).Subtract(orderPlacedDate);
            TimeSpan dateMaxSegmentSize = dateRange.Divide(allStopTypes.Length);
            int stopSequenceIndex = 1;
            DateTime lastMinExpected = orderPlacedDate;
            var (currentStopSequence, isAtStop) = GetSemirandomCurrentStopInfo(allStopTypes, orderStatus);
            (string? customerId, string? locationId)[] locations = GetCustomerLocations(allStopTypes);
            for (int stopSequenceNumber = 1; stopSequenceNumber <= allStopTypes.Length; stopSequenceNumber++)
            {
                string stopType = allStopTypes[stopSequenceNumber - 1];
                (string? customerId, string? locationId) = locations[stopSequenceNumber - 1];
                DateTime minExpected = lastMinExpected.Add(TimeSpan.FromTicks((long)(dateMaxSegmentSize.Ticks * random.NextDouble())));
                lastMinExpected = minExpected;
                DateTime maxExpected = minExpected.Add(TimeSpan.FromTicks((long)(dateMaxSegmentSize.Ticks * random.NextDouble())));
                DateTime? actualArrival = null;
                DateTime? departure = null;
                if (currentStopSequence <= stopSequenceIndex)
                {
                    actualArrival = minExpected.Add(TimeSpan.FromTicks((long)(dateMaxSegmentSize.Ticks * random.NextDouble())));
                }
                if (stopSequenceIndex > currentStopSequence || (stopSequenceIndex == currentStopSequence && !isAtStop))
                {
                    departure = actualArrival?.Add(TimeSpan.FromTicks((long)(dateMaxSegmentSize.Ticks * random.NextDouble())));
                }
                orderStops.Add(new OrderStop
                {
                    OrderId = orderId,
                    StopSequenceNumber = stopSequenceIndex++,
                    StopType = stopType,
                    MinExpectedArrivalTime = minExpected,
                    MaxExpectedArrivalTime = maxExpected,
                    ActualArrivalTime = actualArrival,
                    DepartureTime = departure,
                    Distance = random.Next(100, 4000),
                    ChargePerDistanceUnit = random.Next(50, 500) / 100,
                    CustomerId = customerId,
                    LocationId = locationId
                });
                int cargoChangeSequenceNumber = 1;
                if (stopType == "Pickup" || stopType == "Other cargo change")
                {
                    orderStopCargoChanges.Add(GetDummyOrderStopCargoChange(orderId, stopSequenceIndex, cargoChangeSequenceNumber++, "pickup"));
                }
                if (stopType == "Dropoff" || stopType == "Other cargo change")
                {
                    orderStopCargoChanges.Add(GetDummyOrderStopCargoChange(orderId, stopSequenceIndex, cargoChangeSequenceNumber++, "dropoff"));
                }
            }
            int numberOfAdditionalCharges = random.Next(0, 3);
            string[] additionalChargeTypes = new[] { "Fuel surcharge", "Mileage surcharge", "Special handling", "Documentation fees" };
            for (int additionalChargeIndex = 0; additionalChargeIndex < numberOfAdditionalCharges; additionalChargeIndex++)
            {
                orderAdditionalCharges.Add(new OrderAdditionalCharge
                {
                    OrderId = orderId,
                    AdditionalChargeSequenceNumber = additionalChargeIndex + 1,
                    ChargeDescription = additionalChargeTypes[additionalChargeIndex],
                    ChargeAmount = (decimal)random.Next(100, 100000) / 10
                });
            }
        }
        using FileStream ordersStream = new("orders.csv", FileMode.Create, FileAccess.Write);
        CSVSerializer.Serialize(ordersStream, orders, true);
        using FileStream stopsStream = new("stops.csv", FileMode.Create, FileAccess.Write);
        CSVSerializer.Serialize(stopsStream, orderStops, true);
        using FileStream cargoStream = new("cargo.csv", FileMode.Create, FileAccess.Write);
        CSVSerializer.Serialize(cargoStream, orderStopCargoChanges, true);
        using FileStream chargesStream = new("charges.csv", FileMode.Create, FileAccess.Write);
        CSVSerializer.Serialize(chargesStream, orderAdditionalCharges, true);
    }

    private static string GetRandomGeneralIdentifier()
    {
        if (random.Next(1, 10) <4)
        {
            return "";
        }
        StringBuilder sb = new();
        if (random.Next(0, 1) == 1)
        {
            sb.Append((char)random.Next(65, 90));
        }
        sb.Append(random.NextInt64(1_000, 1_000_000_000_000));
        return sb.ToString();
    }
    
    private static string GetOrderStatus(DateTime orderPlacedDate)
    {
        if (random.Next(1, 13) == 1)
        {
            return "Canceled";
        }
        if (orderPlacedDate < DateTime.Now.Subtract(TimeSpan.FromDays(90)))
        {
            return "Final";
        }
        return new[] { "Not Started", "At Carrier Location", "At Customer Location",
            "At Other Location", "In Transit", "Final" }[random.Next(0, 6)];
    }

    private static int GetWeightedStopCount()
    {
        //I'm sure there's a simpler way to do this
        var weights = new[]
        {
            new {Value = 0, Weight = 12},
            new {Value = 1, Weight = 6},
            new {Value = 2, Weight = 4},
            new {Value = 3, Weight = 3},
            new {Value = 4, Weight = 2},
            new {Value = 5, Weight = 1},
            new {Value = 10, Weight = 1}
        };
        List<int> options = new();
        foreach (var weight in weights)
        {
            for(int i =0; i < weight.Weight; i++)
            {
                options.Add(weight.Value);
            }
        }
        return options[random.Next(0, options.Count - 1)];

    }

    private static (string customerId, string locationId) GetRandomCustomerLocation(bool isPickupRequired, bool isDropoffRequired)
    {
        var locations = new[] {
            ("000001","PLANT",true,false),
            ("000001","OFFICE",false, false),
            ("000001","WHSE",true, true),
            ("000002","#0001", false, true),
            ("000003","MAIN",false, false),
            ("000004","DC",true, false),
            ("000004","OFFICE",false, false),
            ("000004","#001", false, true),
            ("000004","#002",false, true),
            ("000004","#003",false, true),
            ("000004","#005",false, true),
            ("000005","MAIN",false, true),
            ("000006","#1",false, true),
            ("000007","OFC",false, false),
            ("000008","#0001",false, true),
            ("000008","#0002",false, true),
            ("000008","#0003", false, true),
            ("000008","WAREHOUSE", true, true),
            ("000008","#0004",false, true),
            ("000009","IFMN-PLANT",true, false),
            ("000010","HOME",true, false)
        };
        return
            locations
            .Where(loc => isPickupRequired ? loc.Item3 : true && isDropoffRequired ? loc.Item4 : true)
            .Select(loc => (customerId: loc.Item1, locationId: loc.Item2))
            .OrderBy(loc => random.Next())
            .First();

    }

    private static (int currentStopSequence, bool isAtStop) GetSemirandomCurrentStopInfo(string[] stopTypes, string orderStatus)
    {
        //stop sequences are 1-based
        if (orderStatus == "Not Started" || orderStatus == "Canceled")
        {
            return (0, false);
        }
        if (orderStatus == "At Customer Location")
        {
            string[] customerStopTypes = stopTypes.Where(st => st == "Dropoff" || st == "Pickup" || st == "Other cargo change").ToArray();
            if (!customerStopTypes.Any())
            {
                throw new ArgumentException("Must have at least two customer stop types", nameof(stopTypes));
            }
            return (random.Next(0, customerStopTypes.Length - 1) + 1, true);
        }
        if (orderStatus == "At Carrier Location" || orderStatus == "At Other Location")
        {
            string[] carrierStopTypes = stopTypes.Where(st => st == "Carrier-related").ToArray();
            if (!carrierStopTypes.Any())
            {
                return (0, true);
            }
            return (random.Next(0, carrierStopTypes.Length - 1) + 1, true);
        }
        if (orderStatus == "In Transit")
        {
            return (random.Next(stopTypes.Length - 2) + 1, false); //-2 because leaving the final drop-off wouldn't be in-transit
        }
        if (orderStatus == "Final")
        {
            return (stopTypes.Length /* -1 + 1 */, false);
        }
        throw new ArgumentException($"Order status '{orderStatus}' not valid", nameof(orderStatus));
    }

    private static (string? customerId, string? locationId)[] GetCustomerLocations(string[] stopTypes)
    {
        List<(string? customerId, string? locationId)> results = new();
        string? previousCustomerId = null;
        string? previousLocationId = null;
        foreach (string stopType in stopTypes)
        {
            if (stopType == "Carrier-related")
            {
                results.Add((null, null));
            }
            while (true) //The while loop is just to retry if you get the same location twice in a row
            {
                (string custId, string locId) = GetRandomCustomerLocation(
                    stopType == "Pickup" || stopType == "Other cargo change",
                    stopType == "Dropoff" || stopType == "Other cargo change");
                if (custId == previousCustomerId && locId == previousLocationId)
                {
                    continue;
                }
                results.Add((custId, locId));
                previousCustomerId = custId;
                previousLocationId = locId;
                break;
            }
        }
        return results.ToArray();
    }

    public static OrderStopCargoChange GetDummyOrderStopCargoChange(string orderId, int stopSequenceNumber, int cargoChangeSequenceNumber,
        string pickupOrDropoff)
    {
        string cargoChangeType =
            pickupOrDropoff == "pickup"
            ? random.NextSingle() < .5
                ? "Load"
                : "Hook"
            : pickupOrDropoff == "dropoff"
                ? random.NextSingle() < .5
                    ? "Unload"
                    : "Drop"
                : throw new ArgumentException("Should be 'pickup' or 'dropoff'", nameof(pickupOrDropoff));

        return new OrderStopCargoChange
        {
            OrderId = orderId,
            StopSequenceNumber = stopSequenceNumber,
            CargoChangeSequenceNumber = cargoChangeSequenceNumber,
            CargoChangeType = cargoChangeType,
            GeneralDescription = GetRandomCargoDescription(),
            Weight = (double)random.Next(10000, 1000000) / 100,
            TruckNumber = GetRandomGeneralIdentifier(),
            TrailerNumber = GetRandomGeneralIdentifier(),
            Driver1Name = GetRandomDriverName(0),
            Driver2Name = GetRandomDriverName(.9F)
        };
    }

    private static string GetRandomCargoDescription()
    {
        string[] availableItems = new[] {"apples", "bricks", "Caesar salad mix", "dogfood", "engine parts", "flash-frozen french fries", "gold",
            "hot dog buns", "ice cream cones", "jogging pants", "krazy glue", "lobsters", "machinery", "nylon stockings", "orange juice",
            "potatoes", "quilts", "radium", "stovepipe hats", "turtle eggs", "uranium", "velvet", "windows", "x-ray machine parts",
            "yoyos", "zippers"};
        int numberOfItemsToSelect = random.Next(1, 5);
        HashSet<string> selectedItems = new(); //If we get a duplicate, just don't add it.
        for (int i = 0; i < numberOfItemsToSelect; i++)
        {
            selectedItems.Add(availableItems[random.Next(0, availableItems.Length - 1)]);
        }
        if (selectedItems.Count == 1)
        {
            return selectedItems.Single();
        }
        string result =string.Join(", ", selectedItems.Take(selectedItems.Count - 1)) + " and " + selectedItems.Last();
        return string.Concat(result.First().ToString().ToUpper(), result.Skip(1).ToString());
    }

    private static string GetRandomDriverName(float nullLikelihood)
    {
        if (random.NextSingle() < nullLikelihood)
        {
            return "";
        }
        string[] availableFirstNames = new[] {"Alfred", "Betty", "Charles", "Danielle", "Eduardo", "Francis", "George", "Harriet", "Isaiah",
            "Janice", "Kevin", "Louise", "Mitchell", "Nancy", "Oliver", "Patricia", "Quincy", "Roberta", "Samuel", "Tammy", "Ulysses",
            "Velma", "William", "Xaviera", "Yancey", "Zöe"};
        string[] availableLastNames = new[] {"Anderson", "Benitez", "Covington", "Dawson", "Edwards", "French", "Grimes", "Harrison", "Irvin",
            "Jackson", "Kern", "Lewis", "Mitchell", "Nelson", "O'Toole", "Perry", "Quasnoy", "Ramakrishnan", "Smith", "Thompson", "Underwood",
            "Velazquez", "Williams", "Xing", "Yarbrough", "Zakaluzny"};
        float middleNameRand = random.NextSingle();
        char middleNameOption = middleNameRand < .3 ? ' ' : middleNameRand < .75 ? 'I' : 'F';
        int firstNameRandomIndex = random.Next(0, availableFirstNames.Length - 1);
        int middleNameRandomIndex = -1;
        while (true)
        {
            middleNameRandomIndex = random.Next(0, availableFirstNames.Length - 1);
            if (middleNameRandomIndex != firstNameRandomIndex)
            {
                break;
            }
        }
        StringBuilder sb = new();
        sb.Append(availableFirstNames[firstNameRandomIndex]);
        sb.Append(' ');
        if (middleNameOption == 'I')
        {
            sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ"[middleNameRandomIndex]);
            sb.Append(". ");
        }
        else if (middleNameOption == 'N')
        {
            sb.Append(availableFirstNames[middleNameRandomIndex]);
            sb.Append(' ');
        }
        //else no middle name at all
        bool hasMultipleLastNames = random.NextSingle() < .2;
        bool lastNamesHyphenated = hasMultipleLastNames && random.NextSingle() < .7;
        int lastName1RandomIndex = random.Next(0, availableLastNames.Length - 1);
        sb.Append(availableLastNames[lastName1RandomIndex]);
        if (hasMultipleLastNames)
        {            
            sb.Append(lastNamesHyphenated ? '-' : ' ');
            int lastName2RandomIndex = -1;
            while (true)
            {
                lastName2RandomIndex = random.Next(0, availableLastNames.Length - 1);
                if (lastName2RandomIndex != lastName1RandomIndex)
                {
                    break;
                }
            }
            sb.Append(availableLastNames[lastName2RandomIndex]);
        }
        return sb.ToString();
        
    }


}