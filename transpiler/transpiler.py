import json

class TargetSpec:
    types: list[str]

    def __init__(self, types):
        self.types = types
    
    def to_dict(self):
        return {
            'types': self.types
        }

class Effect:
    keyword: str
    args: list[str]

    def __init__(self, effect: str):
        self.keyword = effect.split('(')[0]
        self.args = effect.split('(')[1].split(')')[0].split(',')

        print(self.keyword, self.args)

    def to_dict(self):
        return {
            'keyword': self.keyword,
            'args': self.args
        }

class CardData:
    name: str
    types: list[str]
    cost: dict[str, int]
    stats: dict[str, int]
    characteristics: dict[str, list[str]]
    tags: list[str]
    effects: list[Effect]
    targets: list[TargetSpec]
    variables: dict[str, str]

    def __init__(self, name='', cost={}, stats={}, characteristics={}, tags=[], effects=[], targets=[], variables={}):
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
            'effects': [e.to_dict() for e in self.effects],
            'targets': [t.to_dict() for t in self.targets],
            'variables': self.variables
        }

class Transpiler:
    def __init__(self, ast):
        
        self.ast = ast

    def transpile(self) -> list[CardData]:
        return [self.transpile_card(card) for card in self.ast['cards']]

    def transpile_card(self, source) -> CardData:
        
        name = source['name']
        cost = self.transpile_cost(source)
        stats = self.transpile_stats(source)
        characteristics = self.transpile_characteristics(source) 
        tags = self.transpile_tags(source)
        effects = self.transpile_effects(source)
        targets = self.transpile_targets(source)
        variables = self.transpile_variables(source)
        
        types = characteristics.get('types')
        if 'creature' in types:
            targets = [TargetSpec([{'type': 'node'}])]
            effects = [Effect('move_to(self,t0)')]

        return CardData(
            name=name,
            cost=cost,
            stats=stats,
            characteristics=characteristics,
            tags=tags,
            effects=effects,
            targets=targets,
            variables=variables
        )
    
    def transpile_cost(self, source: dict) -> dict[str, int]:
        if 'cost' not in source:
            return {}

        if isinstance(source['cost'], str):
            # ints are generic, wubrg are colored
            expression = source['cost'].lower()
            cost = { }
            for char in expression:

                if char in 'wubrg':
                    cost[char] = 1 if char not in cost else cost[char] + 1
                elif char.isdigit():
                    cost['c'] = int(char)
        
            return cost
        
        return source['cost']
    
    def transpile_characteristics(self, source: dict) -> dict[str, list[str]]:
        characteristics = { } if 'characteristics' not in source else source['characteristics']
        
        if 'type' in source and isinstance(source['type'], str):
            characteristics['types'] = [t for t in source['type'].split(' ') if t]
        
        return characteristics

    def transpile_stats(self, source: dict) -> dict[str, int]:
        stats = { } if 'stats' not in source else source['stats']

        if 'pt' in source and isinstance(source['pt'], str):
            pt = source['pt'].split('/')
            stats['power'] = int(pt[0])
            stats['toughness'] = int(pt[1])
        
        return stats

    def transpile_tags(self, source: dict) -> list[str]:
        return [] if 'tags' not in source else source['tags']

    def transpile_effects(self, source) -> list[Effect]:
        return [] if 'effects' not in source else [Effect(e) for e in source['effects']],
    
    def transpile_targets(self, source) -> list[TargetSpec]:
        return [] if 'targets' not in source else [TargetSpec(target) for target in source['targets']]

    def transpile_variables(self, source) -> dict[str, str]:
        return { } if 'variables' not in source else source['variables']
