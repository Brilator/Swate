namespace Shared

module TermTypes =

    open Shared.Regex
    open System

    module DbDomain =
    
        type Ontology = {
            Name            : string
            CurrentVersion  : string
            Definition      : string
            DateCreated     : System.DateTime
            UserID          : string
        }

        let createOntology name currentVersion definition dateCreated userID = {     
            Name            = name          
            CurrentVersion  = currentVersion
            Definition      = definition    
            DateCreated     = dateCreated   
            UserID          = userID        
        }

        type Term = {
            OntologyName    : string
            Accession       : string
            Name            : string
            Definition      : string
            XRefValueType   : string option
            IsObsolete      : bool
        }

        let createTerm accession ontologyName name definition xrefvaluetype isObsolete = {          
            OntologyName  = ontologyName
            Accession     = accession    
            Name          = name         
            Definition    = definition   
            XRefValueType = xrefvaluetype
            IsObsolete    = isObsolete   
        }

        type TermRelationship = {
            TermID              : int64
            RelationshipType    : string
            RelatedTermID       : int64
        }

    type TermMinimal = {
        /// This is the Ontology Term Name
        Name            : string
        /// This is the Ontology Term Accession 'XX:aaaaaa'
        TermAccession   : string
    } with
        static member create name termAccession = {
            Name            = name
            TermAccession   = termAccession
        }

        static member ofTerm (term:DbDomain.Term) = {
            Name            = term.Name
            TermAccession   = term.Accession
        }

        /// The numberFormat attribute in Excel allows to create automatic unit extensions.
        /// It uses a special input format which is created by this function and should be used for unit terms.
        member this.toNumberFormat = $"0.00 \"{this.Name}\""

        /// The numberFormat attribute in Excel allows to create automatic unit extensions.
        /// The format is created as $"0.00 \"{MinimalTerm.Name}\"", this function is meant to reverse this, altough term accession is lost.
        static member ofNumberFormat (formatStr:string) =
            let unitNameOpt = Regex.parseDoubleQuotes formatStr
            try
                TermMinimal.create unitNameOpt.Value ""
            with
                | :? NullReferenceException -> failwith $"Unable to parse given string {formatStr} to TermMinimal.Name."

        member this.accessionToTSR = this.TermAccession.Split(@":").[0] 
        member this.accessionToTAN = URLs.TermAccessionBaseUrl + this.TermAccession.Replace(@":",@"_")

    type TermSearchable = {
        // Contains information about the term to search itself. If term accession is known, search result is 100% correct.
        Term                : TermMinimal
        // If ParentTerm isSome, then the term name is first searched in a is_a directed search
        ParentTerm          : TermMinimal option
        // Is term ist used as unit, unit ontology is searched first.
        IsUnit              : bool
        // ColIndex in table
        ColIndex            : int
        // RowIndex in table
        RowIndices          : int []
        // Search result
        SearchResultTerm    : DbDomain.Term option
    } with
        static member create term parentTerm isUnit colInd rowIndices= {
            Term                = term
            ParentTerm          = parentTerm
            IsUnit              = isUnit
            ColIndex            = colInd
            RowIndices          = rowIndices
            SearchResultTerm    = None
        }

        member this.hasEmptyTerm =
            this.Term.Name = "" && this.Term.TermAccession = ""