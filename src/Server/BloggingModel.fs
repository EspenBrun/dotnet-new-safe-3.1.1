module BloggingModel

open System
open Microsoft.EntityFrameworkCore
open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type Blog =
    { [<Key>] Id: Guid
      Url: string }

type BloggingContext public (options: DbContextOptions) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable blogs : DbSet<Blog>
    member this.Blogs with get() = this.blogs and set v = this.blogs <- v
