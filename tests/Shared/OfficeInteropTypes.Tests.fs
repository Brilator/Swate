module OfficeInteropTypes

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

open Shared.OfficeInteropTypes

let buildingBlockTypes = testList "buildingBlockTypes" [
    
    testCase "Source Name" <| fun _ ->
        let header = {SwateColumnHeader = BuildingBlockType.Source.toString}
        Expect.isTrue header.isMainColumn "isMainColumn"
        Expect.isTrue header.isSingleCol "isSingleCol"
        Expect.isFalse header.isFeaturedCol "isFeaturedCol"
        Expect.isTrue header.isInputCol "isInputCol"
        Expect.isFalse header.isOutputCol "isOutputCol"
        Expect.isFalse header.isUnitCol "isUnitCol"
        Expect.isFalse header.isTANCol "isTANCol"
        Expect.isFalse header.isTSRCol "isTSRCol"
        Expect.isFalse header.isTermColumn "isTermColumn"

    testCase "Protocol Type" <| fun _ ->
        let header = {SwateColumnHeader = BuildingBlockType.ProtocolType.toString}
        Expect.isTrue header.isMainColumn "isMainColumn"
        Expect.isFalse header.isSingleCol "isSingleCol"
        Expect.isTrue header.isFeaturedCol "isFeaturedCol"
        Expect.isFalse header.isInputCol "isInputCol"
        Expect.isFalse header.isOutputCol "isOutputCol"
        Expect.isFalse header.isUnitCol "isUnitCol"
        Expect.isFalse header.isTANCol "isTANCol"
        Expect.isFalse header.isTSRCol "isTSRCol"
        Expect.isFalse header.isTermColumn "isTermColumn"

        Expect.equal header.getFeaturedColAccession "NFDI4PSO:1000161" ""

    testCase "Parameter [instrument model]" <| fun _ ->
        let header = {SwateColumnHeader = "Parameter [instrument model]"}

        Expect.isTrue header.isMainColumn "isMainColumn"
        Expect.isFalse header.isSingleCol "isSingleCol"
        Expect.isFalse header.isFeaturedCol "isFeaturedCol"
        Expect.isFalse header.isInputCol "isInputCol"
        Expect.isFalse header.isOutputCol "isOutputCol"
        Expect.isFalse header.isUnitCol "isUnitCol"
        Expect.isFalse header.isTANCol "isTANCol"
        Expect.isFalse header.isTSRCol "isTSRCol"
        Expect.isTrue header.isTermColumn "isTermColumn"

        Expect.equal header.tryGetOntologyTerm (Some "instrument model") "tryGetOntologyTerm"

    testCase "Factor [temperature]" <| fun _ ->
        let header = {SwateColumnHeader = "Factor [temperature]"}

        Expect.isTrue header.isMainColumn "isMainColumn"
        Expect.isFalse header.isSingleCol "isSingleCol"
        Expect.isFalse header.isFeaturedCol "isFeaturedCol"
        Expect.isFalse header.isInputCol "isInputCol"
        Expect.isFalse header.isOutputCol "isOutputCol"
        Expect.isFalse header.isUnitCol "isUnitCol"
        Expect.isFalse header.isTANCol "isTANCol"
        Expect.isFalse header.isTSRCol "isTSRCol"
        Expect.isTrue header.isTermColumn "isTermColumn"

        Expect.equal header.tryGetOntologyTerm (Some "temperature") "tryGetOntologyTerm"

    testCase "Characteristic [strain]" <| fun _ ->
        let header = {SwateColumnHeader = "Characteristics [strain]"}

        Expect.isTrue header.isMainColumn "isMainColumn"
        Expect.isFalse header.isSingleCol "isSingleCol"
        Expect.isFalse header.isFeaturedCol "isFeaturedCol"
        Expect.isFalse header.isInputCol "isInputCol"
        Expect.isFalse header.isOutputCol "isOutputCol"
        Expect.isFalse header.isUnitCol "isUnitCol"
        Expect.isFalse header.isTANCol "isTANCol"
        Expect.isFalse header.isTSRCol "isTSRCol"
        Expect.isTrue header.isTermColumn "isTermColumn"

        Expect.equal header.tryGetOntologyTerm (Some "strain") "tryGetOntologyTerm"
]