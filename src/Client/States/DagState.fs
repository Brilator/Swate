namespace Dag

type Model = {
    Default: obj
} with
    static member init() = {
        Default = ""
    }

type Msg =
| DefaultMsg