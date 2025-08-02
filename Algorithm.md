# 🏰 How the Trust Score Works

Imagine you're building a super cool LEGO castle with your friends. This algorithm is like a special scoreboard that figures out who's helping the most and how important their help is.

---

## 🤖 No Robots Allowed!

First, if someone helping is a robot (like a "bot" that just does automatic updates), we don't give them a score. They're just doing their job, not really *contributing* like a person.

> 🛑 They get a special `-1` score, meaning "not a real person."

---

## 🧱 Basic Help Points

We start by giving you points for a few things:

- **Commit Count**  
  This is like how many times you added a new piece or changed something in the LEGO castle. More changes = more points!

- **Lines Added**  
  This is how many new LEGO bricks you put in. Building new stuff is super important, so you get good points for this.

- **Lines Removed**  
  This is like taking out old, broken, or unnecessary LEGO bricks. Cleaning up is also helpful, so you get some points for that too!

- **Net Lines**  
  Basically how many new bricks you added *after* taking out any old ones.

---

## 🕒 Being Active (Recency Factor)

We also care about *when* you last helped.

- If you helped very recently (like in the last few months), you get a **bonus**!  
  It’s like extra credit for being on the team *right now*.

- If you haven't helped in a while (like a year or two), your points start to **"rot"** away.  
  Like a plant wilting if you stop watering it.  
  The longer you're gone, the more your score goes down — but it never hits zero.

---

## ⏳ Sticking Around (Longevity Factor)

We also give you a little bonus if you've been helping with the castle for a long, long time.  
It shows you're *committed*!

---

## 🚗 "Drive-By" Helper Check

Sometimes someone just adds one LEGO brick and then disappears forever.  
We call them *"drive-by"* helpers.

- If someone only has one "last help date" and "first help date" (meaning they only helped once), and they were supposed to get a big recent-helper bonus, we give them a **small penalty**.
- We don't want to "kick them while they're down," but we also want to be fair to people who stick around.

---

## 🧼 No Cheating (Anti-Abuse Check)

We also have a rule to make sure no one tries to cheat the system.

- If someone makes a **ton of tiny changes** (like adding just one LEGO brick 100 times), we check if they're actually doing *real* work.
- If their changes are super small and super frequent, they may get a **penalty**.

> We want big, meaningful changes — not just lots of little ones to trick the scoreboard!

---

## 🏆 Fairness for Superstars (Capping the Score)

Imagine you have a few friends who are super good at building LEGO castles.

- One friend, "Jaex," built like a **few million bricks**
- Another, "BrycensRanch," built maybe **250,000**

Their raw scores would be huge and really far apart! To make it fair:

### The Cap Rule:
- If your score is **≤ 1000**, you keep your exact score.
- If your score is **> 1000**, we use a **special math trick**:
  > "Okay, you're amazing, but your score will go up slower now."

So even if Jaex built a few million bricks and BrycensRanch built 250k, their final scores won’t be *insanely* far apart.

This keeps the scoreboard fair and motivational for everyone!

---

## 🎯 Summary

That’s how we figure out everyone’s special **"helpfulness" score** for our LEGO castle! 🧱✨

## 🛡️ Platform Trust Score: Who Do We Trust to Build the Best LEGO Plugins?

So, we’ve got our helpful builders, but what about the **plugin publishers**?  
Some people publish amazing stuff and others... well... they try.  
This score helps us figure out **how trustworthy a publisher is** — like checking if their LEGO kit won’t fall apart when you pick it up!

---

### 📦 What We Measure

We look at how active, popular, and well-reviewed a publisher's plugins are. Here's what counts:

- **Plugin Uploads**: Publishing more plugins means you’re trying to help. Big points!
- **Plugin Downloads**: If people are downloading your stuff, that’s a *huge* sign of trust.
- **Plugin Reviews**: If your plugins get lots of positive reviews, that’s awesome. You’re making the community better!
- **General Publisher Reviews**: Outside your plugins, what do people think of *you*?
- **Report Outcomes**: If you report bugs or issues and they turn out to be right — bonus!  
  If your reports are wrong or spammy? Not so much.

---

### ⚖️ How the Scoring Works

We don’t just count raw numbers — that wouldn’t be fair to smaller publishers.  
Instead, we use **logarithmic scaling** (aka: "more = better, but not *infinitely* better").

Then we multiply that by how important each thing is:

| Factor                        | Weight     | Why it matters |
|------------------------------|------------|----------------|
| Plugin Uploads               | `500`      | Shows you’re actively contributing |
| Plugin Downloads             | `450`      | Proof people want what you're making |
| Plugin Positive Reviews      | `50`       | Great feedback = solid plugins |
| Plugin Negative Reviews      | `-300`     | Uh-oh... not everyone’s happy |
| Publisher Positive Reviews   | `350`      | Good overall reputation |
| Publisher Negative Reviews   | `-300`     | Uh... maybe you're hard to work with? |
| Valid Bug Reports            | `350`      | Thanks for helping us fix things! |
| Invalid/Rejected Reports     | `-450`     | Stop crying wolf 🐺🚫 |

---

### 💥 Why Not Just Reward Raw Numbers?

If one publisher has 1,000,000 downloads and another has 25,000, the score difference would be ridiculous.  
So we **cap the impact using logarithmic math**, like this:

> A huge number gets *flattened*, but still contributes a lot.

That way:
- Newer or smaller publishers still have a shot at competing.
- The leaderboard doesn’t just become “the same three people forever.”

---

### 🧮 Final Trust Score Equation

We combine everything like this:

```csharp
Trust Score =
  + Plugin Count × 500  
  + Plugin Downloads × 450  
  + Plugin Positive Reviews × 50  
  - Plugin Negative Reviews × 300  
  + Publisher Positive Reviews × 350  
  - Publisher Negative Reviews × 300  
  + Valid Reports × 350  
  - Invalid Reports × 450
```

> 🚨 If the final score is negative? We round it up to zero. Everyone starts with a clean(ish) slate. 

## ✅ So What’s the Point?

This Platform Trust Score tells us which publishers are:


- Consistent contributors

- Well-liked by the community

- Responsible participants

- Not spamming or gaming the system

Basically — the kind of LEGO engineers you'd trust with the instruction manual. 🧱📘