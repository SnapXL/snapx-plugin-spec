using Jint;
using Jint.Runtime.Interop;
using NativeAOTJS;


var source = new MyEventSource();

var engine = new Engine(options =>
{
    options.Strict();                      // Enable strict mode, improves performance
    // options.LimitRecursion(100);          // Limit recursion depth
    // options.TimeoutInterval(TimeSpan.FromSeconds(1)); // Execution timeout
    // options.MaxStatements(1_000);         // Limit number of executed statements
    options.AllowClr(typeof(Console).Assembly, typeof(Program).Assembly, typeof(MyEventSource).Assembly, typeof(Newtonsoft.Json.JsonConvert).Assembly); // Enable access to .NET types if needed

    options.ExperimentalFeatures = ExperimentalFeature.All;
}).SetValue("source", source);
engine = engine.SetValue("Newtonsoft", TypeReference.CreateTypeReference<Newtonsoft.Json.JsonConverter>(engine));

engine.Execute(@"
var log = System.Console.WriteLine;
            // Template literals
            const name = 'Jint';
            log(`Hello from ${name}`);

            // Arrow functions
            const double = x => x * 2;
            log(`Double 4 = ${double(4)}`);

            // Destructuring
            const [x, y] = [10, 20];
            const { a, b } = { a: 1, b: 2 };
            log(`Destructured: x=${x}, y=${y}, a=${a}, b=${b}`);

            // Class & inheritance
            class Animal {
                constructor(name) {
                    this.name = name;
                }
                speak() {
                    return `${this.name} makes a noise.`;
                }
            }

            class Dog extends Animal {
                speak() {
                    return `${this.name} barks.`;
                }
            }

            const d = new Dog('Rex');
            log(d.speak());

            // Rest/spread
            function sum(...nums) {
                return nums.reduce((acc, n) => acc + n, 0);
            }
            log(`Sum = ${sum(1, 2, 3, 4)}`);

            // Map / Set
            const map = new Map();
            map.set('foo', 123);
            log(`Map foo = ${map.get('foo')}`);

            const set = new Set([1, 2, 2, 3]);
            log(`Set size = ${set.size}`);

            // Symbol
            const sym2 = Symbol('id');
            log(`Symbol: ${sym2.toString()}`);

            // Promise (sync example)
            new Promise(resolve => resolve('Resolved'))
                .then(val => log(`Promise result: ${val}`));
        ");

engine.Execute(@"
var log = System.Console.WriteLine;
            function* counter() {
                let i = 0;
                while (i < 4) {
                    log('yielding: ' + i);
                    yield i++;
                }
            }

            log('--- gen1 (for..of) ---');
            const gen1 = counter();
            for (const value of gen1) {
                log('Generator yield: ' + value);
            }

            log('--- gen2 (manual) ---');
const gen2 = counter();
let result;

result = gen2.next();
log('Next: ' + result.value);

result = gen2.next();
log('Next: ' + result.value);

result = gen2.next();
log('Next: ' + result.value);

result = gen2.next();
log('Done? ' + result.done);
var file = new System.IO.StreamWriter('log.txt');
file.WriteLine('Hello World !');
            // Create a delegate matching EventHandler<string> signature
            const handler = function(sender, message) {
                log('JS received message: ' + message);
source.remove_Message(handler);

            };

            // Subscribe to event
            source.add_Message(handler);
        ");
source.Raise("Hello from .NET");
engine.Execute("""
var Newtonsoft = importNamespace('Newtonsoft.Json');
var json = '{\"name\":\"romvnly\",\"age\":30}';
var obj = Newtonsoft.JsonConvert.DeserializeObject(json);
log('Name: ' + obj.name);
log('Age: ' + obj.age);
"""
);
engine.Execute("""
               log("=== ECMAScript 2023 Features ===");
               
               // 1. Array `findLast` and `findLastIndex`
               const numbers = [1, 2, 3, 4, 5, 6];
               log("findLast even: " + numbers.findLast(n => n % 2 === 0));         // 6
               log("findLastIndex even: " + numbers.findLastIndex(n => n % 2 === 0)); // 5
               
               // 2. `toSorted`, `toReversed`, `toSpliced`
               const original = [5, 3, 1, 4];
               log("original: " + original);
               log("toSorted: " + original.toSorted((a, b) => a - b)); // [1, 3, 4, 5]
               log("toReversed: " + original.toReversed());            // [4, 1, 3, 5]
               log("toSpliced: " + original.toSpliced(1, 2, 9, 8));     // [5, 9, 8, 4]
               
               // 3. `Symbol.prototype.description`
               const sym = Symbol("hello");
               log("Symbol description: " + sym.description); // "hello"
               
               // 4. `ArrayBuffer.prototype.transfer` (stage 3, may be partially implemented)
               try {
                   const buffer = new ArrayBuffer(8);
                   const view = new Uint8Array(buffer);
                   view[0] = 42;
                   // May or may not be supported in Jint depending on version
                   const newBuffer = buffer.transfer(16);
                   log("Transferred buffer byteLength: " + newBuffer.byteLength);
               } catch (e) {
                   log("ArrayBuffer.transfer unsupported: " + e.message);
               }
               
               // 5. Hashbang grammar (not runnable in runtime eval, but part of ES2023)
               /*
               #! /usr/bin/env node
               console.log("Hashbang ignored in compliant environments");
               */
               
               // 6. `Change Array.prototype` methods to not return `this`
               const result2 = [1, 2, 3].with(1, 99); // proposed in ES2022, included in ES2023 final
               log("Array.with: " + result2); // [1, 99, 3]
               
               // 7. Error cause
               try {
                   try {
                       throw new Error("Original error");
                   } catch (e) {
                       throw new Error("Wrapped error", { cause: e });
                   }
               } catch (e) {
                   log("Error: " + e.message);
                   log("Cause: " + (e.cause ? e.cause.message : "none"));
               }
               """);