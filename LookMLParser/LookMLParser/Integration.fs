﻿namespace LookMLParser

module Integration = 
    open LookMLParser.FieldModel;
    open LookMLParser.View;

    let (|Found|_|) key map =
      map
      |> Map.tryFind key
      |> Option.map (fun x -> x, Map.remove key map)

    let convert_into_sql_table name_string = 
        SqlTable {name = name_string}

    let convert_into_field input_map = 

        let dimension_type = 
            match input_map with 
            | Found "dimension" input_map -> DimensionType
            | Found "measure" input_map  -> MeasureType
            | _ -> DimensionType

        let data_type = 
            match dimension_type with
            | DimensionType -> 
                match input_map with 
                | Found "number" input_map -> DimensionDataType DimensionDataType.Number
                | Found "location" input_map -> DimensionDataType DimensionDataType.Location
                | Found "string" input_map -> DimensionDataType DimensionDataType.String
                | Found "tier" input_map -> DimensionDataType DimensionDataType.Tier
                | Found "time" input_map -> DimensionDataType DimensionDataType.Time
                | Found "yesno" input_map -> DimensionDataType DimensionDataType.YesNo
                | Found "zipcode" input_map -> DimensionDataType DimensionDataType.ZipCode
                | _ -> DimensionDataType DimensionDataType.String

            | MeasureType -> 
                match input_map with 
                | Found "sum"  input_map -> MeasureDataType MeasureDataType.Number
                | _ -> MeasureDataType MeasureDataType.Sum

            | _ -> DimensionDataType DimensionDataType.String

        let sql_text = 
            match input_map.TryFind "name" with 
                | Some text -> text
                | None -> "no sql given!!"

        let field_details = {
            label = input_map.TryFind "name";
            sql = sql_text;
            view_label = Some "Test";
            description = Some "Test";
            hidden = false;
            alias = None;
            required_fields = None;
            drill_fields = None
        }

        match (dimension_type, data_type) with 
            | (DimensionType, DimensionDataType parsed_data_type) -> 
                let details = {
                    data_type = parsed_data_type;
                    primary_key = true;
                    alpha_sort = true;
                    tiers = None;
                    style = None ;
                    suggestable = true
                }
                let output = Dimension (DimensionType , details , field_details)
                Some output
                 
            | (MeasureType, MeasureDataType parsed_data_type) ->
                let details = {
                    data_type = parsed_data_type;
                    direction = Some Row;
                    approximate = Some false;
                    approximate_threshold = Some 0;
                    sql_distinct_key = None;
                    list_field = None;
                    filters = None
                }
                Some (Measure (MeasureType , details , field_details))

            | ( _ , _ ) -> None
