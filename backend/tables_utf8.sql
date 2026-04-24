use database bus_booking_db;

CREATE TABLE "Stations" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "City" text NOT NULL,
    "State" text NOT NULL,
    "Code" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Stations" PRIMARY KEY ("Id")
);


CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "FullName" text NOT NULL,
    "Email" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "Phone" text NOT NULL,
    "Role" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);


CREATE TABLE "Routes" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "SourceStationId" uuid NOT NULL,
    "DestinationStationId" uuid NOT NULL,
    "DistanceKm" double precision NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Routes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Routes_Stations_DestinationStationId" FOREIGN KEY ("DestinationStationId") REFERENCES "Stations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Routes_Stations_SourceStationId" FOREIGN KEY ("SourceStationId") REFERENCES "Stations" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "BusOperators" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "CompanyName" text NOT NULL,
    "LicenseNumber" text NOT NULL,
    "ContactPhone" text NOT NULL,
    "Address" text NOT NULL,
    "Status" integer NOT NULL,
    "ApprovedBy" uuid,
    "ApprovedAt" timestamp with time zone,
    "RejectionReason" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_BusOperators" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusOperators_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Notifications" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Title" text NOT NULL,
    "Message" text NOT NULL,
    "Type" text NOT NULL,
    "IsRead" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Token" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "IsRevoked" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "RouteStops" (
    "Id" uuid NOT NULL,
    "RouteId" uuid NOT NULL,
    "StationId" uuid NOT NULL,
    "StopOrder" integer NOT NULL,
    "ArrivalOffsetMinutes" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_RouteStops" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RouteStops_Routes_RouteId" FOREIGN KEY ("RouteId") REFERENCES "Routes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RouteStops_Stations_StationId" FOREIGN KEY ("StationId") REFERENCES "Stations" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Buses" (
    "Id" uuid NOT NULL,
    "OperatorId" uuid NOT NULL,
    "BusNumber" text NOT NULL,
    "BusName" text NOT NULL,
    "BusType" text NOT NULL,
    "TotalSeats" integer NOT NULL,
    "Status" integer NOT NULL,
    "IsAvailable" boolean NOT NULL,
    "ApprovedBy" uuid,
    "ApprovedAt" timestamp with time zone,
    "RejectionReason" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Buses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Buses_BusOperators_OperatorId" FOREIGN KEY ("OperatorId") REFERENCES "BusOperators" ("Id") ON DELETE CASCADE
);


CREATE TABLE "SeatLayouts" (
    "Id" uuid NOT NULL,
    "BusId" uuid NOT NULL,
    "RowNumber" integer NOT NULL,
    "ColumnLabel" text NOT NULL,
    "SeatLabel" text NOT NULL,
    "SeatType" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_SeatLayouts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SeatLayouts_Buses_BusId" FOREIGN KEY ("BusId") REFERENCES "Buses" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Trips" (
    "Id" uuid NOT NULL,
    "BusId" uuid NOT NULL,
    "RouteId" uuid NOT NULL,
    "DepartureDateTime" timestamp with time zone NOT NULL,
    "ArrivalDateTime" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Trips" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Trips_Buses_BusId" FOREIGN KEY ("BusId") REFERENCES "Buses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Trips_Routes_RouteId" FOREIGN KEY ("RouteId") REFERENCES "Routes" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Bookings" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "TripId" uuid NOT NULL,
    "BookingDate" timestamp with time zone NOT NULL,
    "TotalAmount" numeric NOT NULL,
    "Status" integer NOT NULL,
    "CancellationReason" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Bookings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Bookings_Trips_TripId" FOREIGN KEY ("TripId") REFERENCES "Trips" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Bookings_Users_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "TripPricings" (
    "Id" uuid NOT NULL,
    "TripId" uuid NOT NULL,
    "SeatType" integer NOT NULL,
    "Price" numeric NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_TripPricings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TripPricings_Trips_TripId" FOREIGN KEY ("TripId") REFERENCES "Trips" ("Id") ON DELETE CASCADE
);


CREATE TABLE "BookingSeats" (
    "Id" uuid NOT NULL,
    "BookingId" uuid NOT NULL,
    "SeatLayoutId" uuid NOT NULL,
    "PassengerName" text NOT NULL,
    "Age" integer NOT NULL,
    "Gender" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_BookingSeats" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BookingSeats_Bookings_BookingId" FOREIGN KEY ("BookingId") REFERENCES "Bookings" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BookingSeats_SeatLayouts_SeatLayoutId" FOREIGN KEY ("SeatLayoutId") REFERENCES "SeatLayouts" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Payments" (
    "Id" uuid NOT NULL,
    "BookingId" uuid NOT NULL,
    "Amount" numeric NOT NULL,
    "Method" text NOT NULL,
    "TransactionId" text,
    "Status" integer NOT NULL,
    "PaidAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Payments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Payments_Bookings_BookingId" FOREIGN KEY ("BookingId") REFERENCES "Bookings" ("Id") ON DELETE CASCADE
);


CREATE INDEX "IX_Bookings_CustomerId" ON "Bookings" ("CustomerId");


CREATE INDEX "IX_Bookings_TripId" ON "Bookings" ("TripId");


CREATE INDEX "IX_BookingSeats_BookingId" ON "BookingSeats" ("BookingId");


CREATE INDEX "IX_BookingSeats_SeatLayoutId" ON "BookingSeats" ("SeatLayoutId");


CREATE INDEX "IX_Buses_OperatorId" ON "Buses" ("OperatorId");


CREATE UNIQUE INDEX "IX_BusOperators_UserId" ON "BusOperators" ("UserId");


CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");


CREATE UNIQUE INDEX "IX_Payments_BookingId" ON "Payments" ("BookingId");


CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");


CREATE INDEX "IX_Routes_DestinationStationId" ON "Routes" ("DestinationStationId");


CREATE INDEX "IX_Routes_SourceStationId" ON "Routes" ("SourceStationId");


CREATE INDEX "IX_RouteStops_RouteId" ON "RouteStops" ("RouteId");


CREATE INDEX "IX_RouteStops_StationId" ON "RouteStops" ("StationId");


CREATE INDEX "IX_SeatLayouts_BusId" ON "SeatLayouts" ("BusId");


CREATE INDEX "IX_TripPricings_TripId" ON "TripPricings" ("TripId");


CREATE INDEX "IX_Trips_BusId" ON "Trips" ("BusId");


CREATE INDEX "IX_Trips_RouteId" ON "Trips" ("RouteId");



