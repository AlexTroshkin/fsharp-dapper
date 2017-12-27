open Expecto

[<Tests>]
let tests =
  test "A simple test" {
    let subject = "Hello World"
    Expect.equal subject "Hello World" "The strings should equal"
  }

[<EntryPoint>]
let main argv =
    runTestsInAssembly defaultConfig argv