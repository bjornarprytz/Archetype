class TargetSpec:
    types: list[str]

    def __init__(self, types):
        self.types = types

class CardData:
    name: str
    types: list[str]
    cost: dict[str, int]
    stats: dict[str, int]
    characteristics: dict[str, list[str]]
    tags: list[str]
    effects: list[str]
    targets: list[TargetSpec]
    variables: dict[str, str]

    def __init__(self, name, cost, stats, characteristics, tags, effects, targets, variables):
        self.name = name
        self.cost = cost
        self.stats = stats
        self.characteristics = characteristics
        self.tags = tags
        self.effects = effects
        self.targets = targets
        self.variables = variables

    def to_dict(self):
        return {
            'name': self.name,
            'cost': self.cost,
            'stats': self.stats,
            'characteristics': self.characteristics,
            'tags': self.tags,
            'effects': self.effects,
            'targets': [target.__dict__ for target in self.targets],
            'variables': self.variables
        }

class Transpiler:
    def __init__(self, ast):
        print (ast)
        self.ast = ast

    def transpile(self) -> list[CardData]:
        return [self.transpile_card(card) for card in self.ast['cards']]

    def transpile_card(self, card) -> CardData:

        if isinstance(card['cost'], str):
            # ints are generic, wubrg are colored
            expression = card['cost']
            card['cost'] = {  }
            for char in expression.lower():

                if char in 'wubrg':
                    card['cost'][char] = 1 if char not in card['cost'] else card['cost'][char] + 1
                elif char.isdigit():
                    card['cost']['c'] = int(char)

        if 'type' in card:
            if 'characteristics' not in card:
                card['characteristics'] = { }
            card['characteristics']['types'] = [card['type']]

        types = card['characteristics'].get('types')
        if 'creature' in types:
            card['targets'] = [{'type': 'node'}]
            card['effects'] = ['move_to(self,t0)']
            

        return CardData(
            name=card['name'],
            cost=card['cost'],
            stats= { } if 'stats' not in card else card['stats'],
            characteristics={} if 'characteristics' not in card else card['characteristics'],
            tags={} if 'tags' not in card else card['tags'],
            effects=card['effects'],
            targets=[] if 'targets' not in card else [self.transpile_target(target) for target in card['targets']],
            variables={} if 'variables' not in card else card['variables']
        )
    
    def transpile_target(self, target) -> TargetSpec:
        return TargetSpec(
            types=[target['type']] if target['type'] else target['types']
        )


